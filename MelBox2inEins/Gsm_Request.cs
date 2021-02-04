using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2
{
    public static partial class Gsm
    {
        #region Properties
        /// <summary>
        /// Liste der zu sendenden Nachrichten. 
        /// Wird genutzt, um Zeitpunkt für Senden und Modem-Antwort zeitlich zu koordinieren
        /// Tuple<Phone, Message>
        /// int = MessageReference <mr>
        /// </summary>
        internal static List<Sms> SmsSendQueue { get; set; } = new List<Sms>();

        /// <summary>
        /// Objekt der SMS die grade gesendet wird
        /// </summary>
        internal static Sms CurrentSmsSend { get; set; } = null;

        /// <summary>
        /// Liste der SMSen, für die noch Empfangsbestätigungen ausstehen 
        /// BAUSTELLE: Nutzen für Sendewiederholung?
        /// </summary>
        internal static List<Sms> SmsTrackingQueue { get; set; } = new List<Sms>();

        /// <summary>
        /// Merkliste der SMS-Indexe, die zum löschen anstehen
        /// </summary>
        internal static List<int> SmsToDelete = new List<int>();

        #endregion

        #region SMS lesen
        /// <summary>
        /// (Manuelles) Anstoßen des Lesens und Verarbeitens des GSM-Modem-Speichers
        /// </summary>
        public static void ReadGsmMemory()
        {
            SmsDeletePending();
            Gsm_Basics.AddAtCommand("AT+CMGL=\"ALL\"");
        }
        #endregion

        #region SMS versenden
        /// <summary>
        /// Stellt eine zu sendende SMS in die Liste zur Abarbeitung
        /// </summary>
        /// <param name="phone">Telefonnummer an die diese SMS gesendet werden soll (als Zahl mit Ländervorwahl).</param>
        /// <param name="content">Inhalt der SMS, wird ggf. auf max. 160 Zeichen gekürzt.</param>
        public static void SmsSend(ulong phone, string content)
        {
           //Ist diese SMS schon in der Warteschleife?
           List<Sms> results = SmsSendQueue.FindAll(x => x.Phone == phone && x.Message == content);
            
            if (results.Count == 0)
            {
                content.Replace("\r\n", " ");
                if (content.Length > 160) content = content.Substring(0, 160);

                Sms sms = new Sms
                {
                    Message = content,
                    Phone = phone
                };

                SmsSendQueue.Add(sms);
                SmsSendFromList();
            }
        }

        /// <summary>
        /// Sendet die erste SMS aus der Sendeliste
        /// Sperrt weiteres SMS-senden, bis der gesendeten SMS vom Modem eine Referenz zugewiesen wurde.
        /// </summary>
        private static void SmsSendFromList()
        {
            //CurrentSmsSend wird zur Sendefreigabe auf null gesetzt. Siehe Modem-Antwort '+CMGS: <mr>'
            if (SmsSendQueue.Count == 0 ) return;
            if ( CurrentSmsSend != null)
            {
                Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "SMS-Sendeplatz noch nicht frei. Warte auf: " + CurrentSmsSend.Message);               
                return;
            }

            Console.WriteLine("### zu sendende SMSen {0} ###", SmsSendQueue.Count);

            foreach(Sms sms in SmsSendQueue)
            {
                Console.WriteLine("Index: {0}\tAn: {1}\t{2}",sms.Index, sms.Phone, sms.Message);
            }
                        
            CurrentSmsSend = SmsSendQueue.FirstOrDefault();
            SmsSendQueue.Remove(SmsSendQueue.FirstOrDefault()); //.RemoveAt(0);
            GlobalProperty.LastSmsSend = CurrentSmsSend.Message;

            const string ctrlz = "\u001a";

            //Senden
            Gsm_Basics.AddAtCommand("AT+CMGS=\"+" + CurrentSmsSend.Phone + "\"\r");
            Gsm_Basics.AddAtCommand(CurrentSmsSend.Message + ctrlz);

            //Danach warten auf Antwort von GSM-Modem '+CMGS: <mr>' um CurrentSmsSend die Referenz für Empfangsbestätigung zuzuweisen.
            //Nach Zuweisung der Referenz:
            //1) CurrentSmsSend als gesendete SMS in DB schreiben 
            //2) CurrentSmsSend = null setzen 
            //3) diese Methode erneut aufrufen.
        }


        #endregion

        #region SMS löschen
        /// <summary>
        /// Löscht eine SMS aus dem Speicher
        /// </summary>
        /// <param name="smsId">Id der SMS im GSM-Speicher</param>
        static void SmsDelete(int smsId)
        {
            
            Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.SmsStatus, "Die SMS mit der Id " + smsId + " wird gelöscht.");
            

            string cmd = "AT+CMGD=" + smsId;
            //if (Gsm_Com.ATCommandQueue.Contains(cmd)) return;
            Gsm_Basics.AddAtCommand(cmd);
      
            }

        public static void SmsDeletePending()
        {
            //Zum Löschen anstehende SMSen aus GSM-Speicher löschen
            for (int i = 0; i < SmsToDelete.Count; i++)
            {
                SmsDelete(SmsToDelete[0]);
                SmsToDelete.Remove(SmsToDelete[0]);
            }
        }

#endregion

    }
}
