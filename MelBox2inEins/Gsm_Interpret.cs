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

            //EIgene Telefonnumer-Nummer der SIM-Karte auslesen 
            Gsm_Com.AddAtCommand("AT+CNUM");

            //SIM-Karte im Mobilfunknetz registriert?
            Gsm_Com.AddAtCommand("AT+CREG=1");

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

            Gsm_Com.AddAtCommand("AT+CMGL=\"ALL\"");

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
                Gsm_Com.AddAtCommand("AT+CMGL=\"ALL\"");
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
                foreach (int index in SmsToDelete)
                {
                    SmsDelete(index);
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
                Gsm_Com.AddAtCommand("AT+CMGL=\"ALL\"");
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
                Gsm_Com.AddAtCommand("AT+CMGL=\"ALL\"");
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
                Regex r = new Regex(@"^SCKS: (\d+)");
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
                Regex r = new Regex(@"\+CREG: (d+)"); //SAMBA75
                Match m = r.Match(input);

                Console.WriteLine(input);
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

        /// <summary>
        /// Lese SMS-Textnachricht aus Modemspeicher, melde und lösche die Nachricht anschließend.
        /// </summary>
        /// <param name="input"></param>
        private static void ParseSmsMessages(string input)
        {
            if (input == null) return;
            try
            {
                #region comment 
                /*
                +CMGL: <index> ,  <stat> ,  <oa> / <da> , [ <alpha> ], [ <scts> ][,  <tooa> / <toda> ,  <length> ]
                <data>
                [... ]
                OK

                +CMGL: 9,"REC READ","+4917681371522",,"20/11/08,13:47:10+04"
                Ein Test 08.11.2020 13:46 PS sms38.de
                //*/
                #endregion

                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n"); //SAMBA75
                //Regex r2 = new Regex(@"(\d+),(.+\s.+),(.+),(.+),(.+),(.+),(.+)\n{2}"); //SAMSUNG GALAXY A3
                Match m = r.Match(input);

                while (m.Success)
                {
                    Sms sms = new Sms();
                    sms.SetIndex(m.Groups[1].Value);
                    sms.Status = m.Groups[2].Value;
                    sms.Phone = GsmConverter.StrToPhone(m.Groups[3].Value);
                    sms.Name = m.Groups[4].Value;
                    sms.SetTimeStamp(m.Groups[5].Value);
                    sms.Message = m.Groups[6].Value;

                    if (sms.Status == "REC UNREAD" || sms.Status == "REC READ")
                    {
                        RaiseSmsRecievedEvent(sms);
                        SmsToDelete.Add(sms.Index);
                    }

                    m = m.NextMatch();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Lese Statusreport-SMS Modemspeicher, melde und lösche den Statusreport anschließend.
        /// ACHTUNG: Überprüfung des Reports 
        /// </summary>
        /// <param name="input"></param>
        private static void ParseStatusReport(string input)
        {
            if (input == null) return;
            try
            {
                //+CMGL: < index > ,  < stat > ,  < fo > ,  < mr > , [ < ra > ], [ < tora > ],  < scts > ,  < dt > ,  < st >
                //[... ]
                //OK
                //<st> 0-31 erfolgreich versandt; 32-63 versucht weiter zu senden: 64-127 Sendeversuch abgebrochen

                //z.B.: +CMGL: 1,"REC READ",6,34,,,"20/11/06,16:08:45+04","20/11/06,16:08:50+04",0
                //Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",(\d+),(\d+),,,""(.+)"",""(.+)"",(\d+)\r\n");
                Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",(\d+),(\d+),,,""(.+)"",""(.+)"",(\d+)");
                Match m = r.Match(input);

                while (m.Success)
                {
                    Sms smsReport = new Sms();
                    smsReport.SetIndex(m.Groups[1].Value); //<index>
                    smsReport.Status = m.Groups[2].Value; //<stat>
                    smsReport.FirstOctet = byte.Parse(m.Groups[3].Value); //<fo>
                    smsReport.MessageReference = byte.Parse(m.Groups[4].Value); //<mr>
                    //sms.SmsProviderTimeStamp = GsmConverter.ReadDateTime(m.Groups[5].Value); //<scts: ServiceCenterTimeStamp>
                    smsReport.SmsProviderTimeStamp = GsmConverter.ReadDateTime(m.Groups[6].Value); //<dt: DischargeTime>
                    smsReport.SendStatus = byte.Parse(m.Groups[7].Value); //<st>

                    //Für Wiedererkennung Werte aus 'Orignal-SMS' einfügen 
                    Sms smsTracking =  SmsTrackingQueue.Find(x => x.MessageReference == smsReport.MessageReference);
                    if (smsTracking != null)
                    {
                        smsReport.Message = smsTracking.Message;
                        smsReport.Phone = smsTracking.Phone;

                        SmsTrackingQueue.Remove(smsTracking);
                        MelBox2.GlobalProperty.SmsPendingReports = SmsTrackingQueue.Count;
                    }

                    if (smsReport.Status == "REC UNREAD" || smsReport.Status == "REC READ")
                    {
                        RaiseSmsStatusreportEvent(smsReport);
                        SmsToDelete.Add(smsReport.Index);
                    }

                    m = m.NextMatch();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Lese die Referenz einer zuvor gesendeten Nachricht aus und weise sie der gesendeten SMS zu.
        /// Gebe das Senden der nächsten SMS frei.
        /// </summary>
        /// <param name="input"></param>
        private static void ParseSmsReference(string input)
        {
            if (input == null) return;
            //CMGS=[...]    CMSS=[...]
            /* z.B. 
            +CMGS: 67       +CMSS: 67
            OK              OK
            */

            Regex r = new Regex(@"\+CM[GS]S: (\d+)");
            Match m = r.Match(input);

            while (m.Success)
            {
                if (byte.TryParse(m.Groups[1].Value, out byte trackingId))
                {
                    if (CurrentSmsSend == null)
                    {
                        //Fehler: Empfangsbestätigung, aber keine SMS gesendet
                        Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("Erhaltene Referenz {0} konnte keiner gesendeten Nachricht zugewiesen werden.", trackingId), trackingId);
                        continue;
                    }

                    Console.WriteLine("Tracking-ID {0} für Nachricht an:\r\n+{1}\r\n{2}", trackingId, CurrentSmsSend.Phone, CurrentSmsSend.Message);

                    CurrentSmsSend.MessageReference = trackingId;                   
                    CurrentSmsSend.SmsProviderTimeStamp = DateTime.UtcNow; // Timeout Sendungsverfolgung

                    SmsTrackingQueue.Add(CurrentSmsSend);
                    MelBox2.GlobalProperty.SmsPendingReports = SmsTrackingQueue.Count;

                    RaiseSmsSentEvent(CurrentSmsSend);

                    //Wieder frei machen für nächste zu sendende SMS
                    CurrentSmsSend = null;
                    //Nächste Nachricht senden
                    SmsSendFromList();                   
                }

                m = m.NextMatch();
            }
        }
    }
}
