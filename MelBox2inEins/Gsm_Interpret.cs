using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MelBox2;

namespace MelBox2
{
    public partial class Gsm
    {
        #region Fields

        #endregion

        #region Properties
        public static int GsmReadCycleSeconds { get; set; } = 60;

        #endregion

        #region Events
        /// <summary>
        /// Event SMS empfangen
        /// </summary>
        public static event EventHandler<Sms> SmsRecievedEvent;

        /// <summary>
        /// Trigger für das Event SMS empfangen
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsRecievedEvent(Sms e)
        {
            SmsRecievedEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<Sms> SmsSentEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsSentEvent(Sms e)
        {
            SmsSentEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<Sms> SmsStatusreportEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsStatusreportEvent(Sms e)
        {
            SmsStatusreportEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<GsmEventArgs> GsmSignalQualityEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseGsmSignalQualityEvent(GsmEventArgs e)
        {
            GsmSignalQualityEvent?.Invoke(null, e);
        }
        #endregion

        #region Constructor
        public static void Connect()
        {
            Gsm_Com.GsmEvent += ParseGsmRecEvent;
            Gsm_Com.GsmConnected += SetupGsm;
            Gsm_Com.Connect();
        }
        #endregion

        private static void SetupGsm(object sender, GsmEventArgs e)
        {
            //Nur beim Verbinden ausführen
            if (e.Type != GsmEventArgs.Telegram.GsmConnection || !e.Message.ToLower().Contains("verbunden")) return;

            Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "starte GSM-Setup");
            System.Threading.Thread.Sleep(2000); //Angstpause

            //Test, ob Modem antwortet
            Gsm_Com.AddAtCommand("AT");

            //Erzwinge, dass bei Fehlerhaftem SMS-Senden "+CMS ERROR: <err>" ausgegeben wird statt "OK"
            Gsm_Com.AddAtCommand("AT^SM20=0,0");

            //Benachrichtigung bei SIM-Karten erkannt oder Sim-Slot offen/zu
            Gsm_Com.AddAtCommand("AT^SCKS=1");
            Gsm_Com.AddAtCommand("AT^SCKS?");

            //EIgene Telefonnumer-Nummer der SIM-Karte auslesen 
            Gsm_Com.AddAtCommand("AT+CNUM");

            //SIM-Karte im Mobilfunknetz registriert?
            Gsm_Com.AddAtCommand("AT+CREG=1");
            Gsm_Com.AddAtCommand("AT+CREG?");

            //Signalqualität
            Gsm_Com.AddAtCommand("AT+CSQ");

            //Textmode
            Gsm_Com.AddAtCommand("AT+CMGF=1");

            //SendATCommand("AT+CPMS=\"SM\""); //ME, SM, MT
            //SendATCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");
            Gsm_Com.AddAtCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");

            //Sendeempfangsbestätigungen abonieren
            //Quelle: https://www.codeproject.com/questions/271002/delivery-reports-in-at-commands
            //Quelle: https://www.smssolutions.net/tutorials/gsm/sendsmsat/
            //AT+CSMP=<fo> [,  <vp> / <scts> [,  <pid> [,  <dcs> ]]]
            // <fo> First Octet:
            // <vp> Validity-Period: 0 - 143 (vp+1 x 5min), 144 - 167 (12 Hours + ((VP-143) x 30 minutes)), [...]
            Gsm_Com.AddAtCommand("AT+CSMP=49,1,0,0");

            Gsm_Com.AddAtCommand("AT+CNMI=2,1,2,2,1");
            //möglich AT+CNMI=2,1,2,2,1

            ReadGsmMemory();

            //Startet Timer zum wiederholten Abrufen von Nachrichten
            SetCyclicTimer();

            Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "GSM-Setup abgeschlossen.");
        }

        /// <summary>
        /// Waretet eine Zeit und stößt dann das Lesen des GSM-Speichers an
        /// </summary>
        internal static void SetCyclicTimer()
        {
            Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Erste Abfrage: " + DateTime.Now.AddSeconds(GsmReadCycleSeconds));

            System.Timers.Timer aTimer = new System.Timers.Timer(GsmReadCycleSeconds * 1000); //sec
            aTimer.Elapsed += (sender, eventArgs) =>
            {
                Gsm_Com.AddAtCommand("AT+CSQ");
                ReadGsmMemory();
                Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Nächste Abfrage: " + DateTime.Now.AddSeconds(GsmReadCycleSeconds));
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        /// <summary>
        /// Wird bei jedem Empfang von Daten durch COM aufgerufen!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void ParseGsmRecEvent(object sender, GsmEventArgs e)
        {
            if (e.Type != GsmEventArgs.Telegram.GsmRec) return;

            string input = e.Message;

            if (input == null) return;
            if (input == "\r\nOK\r\n") return;

            #region SMS-Verarbeitung
            //COM-Antwort auf Gesendete SMS: Referenzzuweisung für Empfangsbestätigung
            if (input.Contains("+CMGS:") || input.Contains("+CMSS:")) //
            {
                ParseSmsReference(input);
            }

            //Liste der Nachrichten im GSM-Speicher
            if (input.Contains("+CMGL:")) 
            {
                //Empfangsbestätigungen lesen
                ParseStatusReport(input);

                //Empfangen neuer Nachrichten
                ParseSmsMessages(input);

                //Zum Löschen anstehende SMSen aus GSM-Speicher löschen
                for (int i = 0; i < SmsToDelete.Count; i++)
                {
                    SmsDelete(SmsToDelete[0]);
                    SmsToDelete.Remove(SmsToDelete[0]);
                }                
            }

            //Indikator neuen Statusreport empfangen
            if (input.Contains("+CDSI:"))
            {
                /*
                Meldung einer neu eingegangenen Nachricht von GSM-Modem

                Neuen Statusreport empfangen:
                bei AT+CNMI= [ <mode> ][,  <mt> ][,  <bm> ][,  2 ][,  <bfr> ]
                erwartete Antwort: +CDSI: <mem3>, <index>
                //*/
               
                ReadGsmMemory();
            }

            //Indikator neue SMS empfangen
            if (input.Contains("+CMTI:"))
            {
                /*
                Meldung einer neu eingegangenen Nachricht von GSM-Modem

                Neue SMS emfangen:
                bei AT+CNMI= [ <mode> ][,  1 ][,  <bm> ][,  <ds> ][,  <bfr> ]
                erwartete Antwort: +CMTI: <mem3>, <index>				
                //*/
                ReadGsmMemory();
            }

            //Fehlermeldung von Modem bei SMS senden oder Empfangen
            if(input.Contains("+CMS ERROR:"))
            {
                Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Am GSM-Modem ist ein Fehler beim Senden oder Empfangen einer SMSM aufgetreten", input);
            }
            #endregion

            #region SIM-Status
            //GSM-Signalqualität
            if (input.Contains("+CSQ:"))
            {
                //+CSQ: <rssi> , <ber>
                //<rssi> Mögliche Werte: 2 - 9 marginal, 10 - 14 OK, 15 - 19 Good, 20 - 30 Excellent, 99 = kein Signal
                //<ber> Bit-Error-Rate: 0 bis 7 = Sprachqualität absteigend, 99 = unbekannt
                Regex rS = new Regex(@"CSQ:\s([0-9]+),([0-9]+)");
                Match m = rS.Match(input);

                if (!int.TryParse(m.Groups[1].Value, out int rawQuality)) return;
                int qualityPercent = 0;

                if (rawQuality < 99)
                {
                    qualityPercent = rawQuality * 100 / 31;
                }

                while (m.Success)
                {
                    RaiseGsmSignalQualityEvent(new GsmEventArgs(GsmEventArgs.Telegram.GsmSignal, string.Format("GSM Signalstärke {0:00}%", qualityPercent), qualityPercent));
                    m = m.NextMatch();
                }
            }

            //Eigene Rufnummer lesen
            if (input.Contains("+CNUM:"))
            {
                Regex r = new Regex(@"\+CNUM: ""(.+)"",""(.+)"",(.*)"); //SAMBA75
                Match m = r.Match(input);

                Console.WriteLine(input);
                while (m.Success)
                {
                    string name = m.Groups[1].Value;
                    string strPhone = m.Groups[2].Value;

                    Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmOwnPhone, string.Format("Eigene Nummer: '{0}' {1}", name, strPhone), GsmConverter.StrToPhone(strPhone));
                    m = m.NextMatch();
                }
            }

            //SIM-Schubfach / SIM erkannt
            if (input.Contains("^SCKS:"))
            {                
                Regex r = new Regex(@"\^SCKS: (\d+)");
                Match m = r.Match(input);

                while (m.Success)
                {
                    if (int.TryParse(m.Groups[1].Value, out int simCardHolderStatus))
                    {
                        if (simCardHolderStatus == 1) 
                            GlobalProperty.SimDetected= true;
                        else
                            GlobalProperty.SimDetected = false;
                    }
                    m = m.NextMatch();
                }
            }

            //Registrierung im Mobilfunknetz
            if (input.Contains("+CREG:"))
            {
                Regex r = new Regex(@"\+CREG: (\d)"); //SAMBA75
                Match m = r.Match(input);

                while (m.Success)
                {
                    if (int.TryParse(m.Groups[1].Value, out int networkRegStatus))
                    {
                        switch (networkRegStatus)
                        {

                            case 0:
                                GlobalProperty.NetworkRegistrationStatus = "nicht registriert";
                                break;
                            case 1:
                                GlobalProperty.NetworkRegistrationStatus = "registriert";
                                break;
                            case 2:
                                GlobalProperty.NetworkRegistrationStatus = "Netzsuche";
                                break;
                            case 3:
                                GlobalProperty.NetworkRegistrationStatus = "Registrierung abgelehnt";
                                break;
                            case 5:
                                GlobalProperty.NetworkRegistrationStatus = "Roaming";
                                break;
                            default:
                                GlobalProperty.NetworkRegistrationStatus = "unbekannt";
                                break;
                        }
                    }
                    
                    m = m.NextMatch();
                }
                if (input.Contains("+CREG: 0,0"))
                {
                    Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Das GSM-Modem ist nicht im Mobilfunknetz angemeldet.");
                }
            }

            #endregion
        }

      
    }
}
