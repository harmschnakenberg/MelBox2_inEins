using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class Gsm
    {
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
                    Sms smsTracking = SmsTrackingQueue.Find(x => x.MessageReference == smsReport.MessageReference);
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
                throw new Exception(string.Format("ParseStatusReport() {0}\r\n{1}\r\n{2}", ex.GetType(), ex.Message, ex.InnerException) );
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
