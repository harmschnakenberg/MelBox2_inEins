using System;
using System.Collections.Generic;
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

            NetworkCredential credential = new NetworkCredential();
            credential.UserName = Properties.Settings.Default.SmtpUserName;
            credential.Password = Properties.Settings.Default.SmtpUserPassword; // "nqpfrufwrjxnrqih"

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
    }
}
