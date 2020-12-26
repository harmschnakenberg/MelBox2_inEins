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
        public static string SmtpServer { get; set; } = "mail.gmx.net"; // "192.168.165.29";
        public static MailAddress SMSCenter { get; set; } = new MailAddress("SMSZentrale@Kreutztraeger.de", "SMS Zentrale Kreutzträger Kältetechnik");

        public static List<string> PermanentEmailRecievers { get; set; }

        public static void Send(MailAddressCollection to, string body, string subject = "")
        {
            //BAUSTELLE
            return;

            //per SmtpClient Funktioniert.
            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.Host = SmtpServer;
                smtpClient.Port = 465;
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;// CredentialCache.DefaultNetworkCredentials;

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
                        subject = body;
                    int maxSubjectLenth = 255;
                    if (subject.Length > maxSubjectLenth) 
                        message.Subject = subject.Substring(0, maxSubjectLenth).Replace("\r\n", string.Empty);
                    else
                        message.Subject = subject.Replace("\r\n", string.Empty);
                    //Nachricht
                    message.Body = body;

                    try
                    {
                        smtpClient.Send(message);
                    }
                    catch (SmtpException ex)
                    {
                        //provisorisch
                        throw ex;
                    }
                }
            }
        }
    }
}
