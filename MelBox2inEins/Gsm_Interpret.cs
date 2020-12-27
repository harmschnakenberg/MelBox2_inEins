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
                Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Am GSM-Modem ist ein Fehler beim Senden oder Empfangen einer SMSM aufgetreten", input);
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

                    Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmOwnPhone, string.Format("Eigene Nummer: '{0}' {1}", name, strPhone), GsmConverter.StrToPhone(strPhone));
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
                    Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Das GSM-Modem ist nicht im Mobilfunknetz angemeldet.");
                }
            }

            #endregion
        }

      
    }
}
