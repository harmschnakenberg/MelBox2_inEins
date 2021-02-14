using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    class Email
    {
        //public static string SmtpServer { get; set; } = "smtp.gmail.com"; //"mail.gmx.net"; // "192.168.165.29"; //"smtp.gmail.com"
        public static MailAddress SMSCenter { get; set; } = new MailAddress("kreutztraegersmszentrale@gmail.com", "SMS Zentrale Kreutzträger Kältetechnik");//"SMSZentrale@kreutztraeger.de"
        public static MailAddress MelBox2Admin { get; set; } = new MailAddress("harm.schnakenberg@kreutztraeger.de", "MelBox2 Admin");

        public static List<string> PermanentEmailRecievers { get; set; }

       
        public static void Send(MailAddressCollection to, Sms e, bool isBlocked)
        {
            StringBuilder body = new StringBuilder();
            StringBuilder subject = new StringBuilder();

            int contactId = Program.Sql.GetContactId("", e.Phone, "", e.Message);
            DataTable senderContact = Program.Sql.GetViewContactInfo(contactId);
            string senderName = senderContact.Rows[0]["Name"].ToString();
            string senderFirma = senderContact.Rows[0]["Firma"].ToString();

            subject.Append("SMS Eingang >" + senderFirma + "<, ");
            subject.Append("SMS Text >" + e.Message + "<");

            body.Append("\r\nSMS Absender\t>" + e.Phone + ": " + senderName + ", " + senderFirma + "< \r\n"); //SMS Absender >GMX SMS<
            body.Append("SMS Text\t>" + e.Message + "< \r\n");
            body.Append("SMS Sendezeit\t>" + e.SmsProviderTimeStamp + "< \r\n");

            if (isBlocked)
                body.Append("\r\n\r\nKeine Weiterleitung an Bereitschaftshandy.");

            Send(to, body.ToString(), subject.ToString());
        }


        public static void Send(MailAddress to, string body, string subject = "")
        {
            MailAddressCollection addresses = new MailAddressCollection
            {
                to
            };

            Send(addresses, body, subject);
        }

        public static void Send(MailAddressCollection to, string body, string subject = "")
        {

            //BAUSTELLE
            //Console.WriteLine("Email nicht implementiert. Keine gesendet Email an: " + to.ToList().ToArray().ToString());
            //return;

            NetworkCredential credential = new NetworkCredential
            {
                UserName = Properties.Settings.Default.SmtpUserName,
                Password = Properties.Settings.Default.SmtpUserPassword // "nqpfrufwrjxnrqih"
            };

            //per SmtpClient Funktioniert. Muss ggf. als "unsichere App" freigegeben werden.
            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Host = Properties.Settings.Default.SmtpServerName; //SmtpServer;
                smtpClient.Port = Properties.Settings.Default.SmtpPort; //465 //587 //25
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.Credentials = credential; //new NetworkCredential("kreutztraegersmszentrale@gmail.com", "nqpfrufwrjxnrqih");// CredentialCache.DefaultNetworkCredentials; //new NetworkCredential("username", "password");

                using (MailMessage message = new MailMessage())
                {
                    message.From = SMSCenter;

                    foreach (MailAddress toAddress in to)
                    {
                        message.To.Add(toAddress);
                    }

                    //BAUSTELLE SilentListeners hier eintragen?
                    //if (to.Where(x => x.Address == BccReciever).Count() == 0)
                    //{
                    //    message.Bcc.Add(BccReciever);
                    //}

                    //Betreff
                    if (subject.Length < 3)
                    {
                        subject = body;
                    }

                    int maxSubjectLenth = 255;

                    if (subject.Length > maxSubjectLenth)
                    {
                        message.Subject = subject.Substring(0, maxSubjectLenth).Replace("\r\n", string.Empty);
                    }
                    else
                    {
                        message.Subject = subject.Replace("\r\n", string.Empty);
                    }
                    //Nachricht
                    message.Body = body;

                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (SmtpException ex)
                    {
#if DEBUG
                        Console.WriteLine("Fehler: Email nicht gesendet an:");

                        foreach (MailAddress email in to)
                        {
                            Console.WriteLine(email.Address);
                        }

                        Console.WriteLine(ex.GetType() + Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException);
#else
                        //provisorisch
                        throw ex;
#endif
                    }
                }
            }
        }

        #region Sprachanrufe behandeln

        private static bool VoiceCallRecieved { get; set; } = false;


        public static void VoiceCallRing()
        {
            if (!VoiceCallRecieved)
            {
#if DEBUG
                Console.WriteLine("Eingehenden Anruf registriert.");
#endif
                VoiceCallRecieved = true;
                SendVoiceCallRecievedNotification();
            }
        }

        private static void SendVoiceCallRecievedNotification()
        {
            string subject = "Sprachanruf > ";
            string body = string.Format("Sprachanruf {0} weitergeleitet. ", DateTime.Now);

            MailAddressCollection to = new MailAddressCollection();
            foreach (var address in PermanentEmailRecievers)
            {
                to.Add(address);
            }

            Send(to, body, subject);

#if DEBUG
            Console.WriteLine("Send Email: 'Eingehender Anruf' und sperre erneute E-mail für 30 sec.");
#endif

            var t = Task.Run(async delegate
            {
                await Task.Delay(30000); //es wird maximal alle 30 sec eine mail rausgeschickt.                
            });
            t.Wait();

            VoiceCallRecieved = false;
        }

#endregion
    }
}
