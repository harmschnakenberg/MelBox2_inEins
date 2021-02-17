using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxSql
    {
        #region Nachrichten

        /// <summary>
        /// Ermittelt die ID für den Text der Nachricht. Erstellt für neuen Text eine ID.
        /// </summary>
        /// <param name="content">Nachrichtentext</param>
        /// <returns>ID für den übergebenen content</returns>
        public int GetContentId(string content)
        {
            try
            {
                const string query = @"INSERT INTO MessageContent(Content) SELECT $Content WHERE NOT EXISTS(SELECT 1 FROM MessageContent WHERE Content = $Content); " +
                                "SELECT ID FROM MessageContent WHERE Content = $Content; ";

                if (content == null) content = "-KEIN TEXT-";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "$Content", content }
                };
                
                return SqlSelectInteger(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetMessageId(content)" + ex.GetType() + "\r\n" + ex.Message);
            }
        }


        /// <summary>
        /// Ermittelt die eindeutige Nummer des Nachrichteninhalts der empfangenen Nachricht.
        /// </summary>
        /// <param name="recMsgId">Eindeutige Nummer der empfangenen Nachricht</param>
        /// <returns>eindeutige Nummer des Nachrichteninhalts der empfangenen Nachricht</returns>
        public int GetContentId(int recMsgId)
        {
            try
            {
                const string query = @"SELECT ContentId FROM LogRecieved WHERE Id = $recMsgid; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "$recMsgid", recMsgId }
                };

                return SqlSelectInteger(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetContentId(recMsgId)" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        #endregion

        #region Kontakte

        /// <summary>
        /// Versucht den Kontakt anhand der Telefonnummer, email-Adresse oder dem Beginn eriner Nachricht zu identifizieren.
        /// Kann kein Kontakt ermittelt werden, wird ein neuer Kontakt in der Datenbank angelegt.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <param name="message">Extrahiert KeyWords, falls andere Angaben nicht helfen</param>
        /// <returns>Id des Kontakts in der Datenbank</returns>
        public int GetContactId(string name = "", ulong phone = 0, string email = "", string message = "")
        {
            try
            {
                const string query = @"SELECT Id " +
                                      "FROM Contact " +
                                      "WHERE  " +
                                      "( length(Name) > 0 AND Name = @name ) " +
                                      "OR ( Phone > 0 AND Phone = @phone ) " +
                                      "OR ( length(Email) > 0 AND Email = @email ) " +
                                      "OR ( length(KeyWord) > 0 AND KeyWord = @keyWord ) ";

                string keyWords = ExtractKeyWords(message);

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@name", name },
                    { "@phone", phone },
                    { "@email", email },
                    { "@message", message },
                    { "@keyWord", keyWords }

                };

                int contactId = SqlSelectInteger(query, args);

                if (contactId == 0)
                {
                    //Neuen Eintrag erstellen
                    contactId = InsertContact(name, null, 1, email, phone, 0, false, false);
                    Email.Send(Email.MelBox2Admin, string.Format("Es wurde ein neuer Kontakt in die Datenbank aufgenommen\r\nName: {2}\r\nEmail: {0}\r\nTelefon: +{1}\r\nNachricht: {3}\r\n\r\nBitte den Kontakt in der Datenbank vervollständigen.", email, phone, name, message), "MelBox2 - neuer Kontakt");
                }

                return contactId;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetContactId()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public int GetContactIdFromLogin(string name, string password)
        {
            try
            {
                const string query = "SELECT \"Id\" FROM \"Contact\" WHERE Name = @name AND ( Password = @password OR Password IS NULL )";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@name", name },
                    { "@password",Encrypt(password)}
                };

                return SqlSelectInteger(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("GetContactIdFromLogin()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public DataTable GetContactFromLogin(string name, string password)
        {
            try
            {
                const string query = "SELECT * FROM \"Contact\" WHERE Name = @name AND ( Password = @password OR Password IS NULL )";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@name", name },
                    { "@password",Encrypt(password)}
                };

                return SqlSelectDataTable("Benutzer", query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("GetContactFromLogin()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Liste der Ids und Namen von Kontakten, die per Email oder SMS erreichbar sind
        /// </summary>
        /// <returns></returns>
        public DataTable GetContactList()
        {
            string query = "SELECT Contact.Id AS ContactId, Contact.Name AS Name " + //'********' AS Passwort, CompanyId, Company.Name AS Firma, Email, Phone AS Telefon, Contact.SendSms AS SendSms , Contact.SendEmail AS SendEmail, MaxInactiveHours AS Max_Inaktivität " +
                           "FROM \"Contact\" JOIN \"Company\" ON CompanyId = Company.Id " +
                           "WHERE Contact.SendSms > 0 OR Contact.SendEmail > 0; ";

            return SqlSelectDataTable("Kontakte", query);
        }

        public DataTable GetMonitoredContactList()
        {
            string query = "SELECT Contact.Id AS Id, Contact.Name AS Name, Company.Name AS Firma, MaxInactiveHours ||' Stunden' AS Max_Inaktiv " +
                           "FROM \"Contact\" JOIN \"Company\" ON CompanyId = Company.Id " +
                           "WHERE MaxInactiveHours > 0; ";

            return SqlSelectDataTable("Überwachte Kontakte", query);
        }

        //public DataTable GetContactInfo(int contactId)
        //{
        //    string query = "SELECT Contact.Name AS Name, Company.Name AS Firma, Contact.Phone AS Phone, Contact.Email AS Email " +
        //                   "FROM \"Contact\" JOIN \"Company\" ON CompanyId = Company.Id " +
        //                   "WHERE Contact.Id = @contactId; ";

        //    Dictionary<string, object> args = new Dictionary<string, object>
        //        {
        //            { "@contactId", contactId }
        //        };

        //    return SqlSelectDataTable("Kontakt", query, args);
        //}


        public int GetLastCompany()
        {
            try
            {
                const string query = "SELECT Id FROM \"Company\" ORDER BY Id DESC LIMIT 1; ";

                return SqlSelectInteger(query);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetLastCompany() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        #endregion

        #region Bereitschaft

        public bool IsMessageBlockedNow(string Message)
        {
            int contentId = GetContentId(Message);

            if (contentId == 0) return false;

            const string query = @"SELECT Days FROM BlockedMessages " +
                                  "WHERE Id = $contentId AND " +
                                  "(StartHour = EndHour OR " +
                                  "CAST(strftime('%H','now', 'localtime') AS INTEGER) > StartHour OR " +
                                  "CAST(strftime('%H','now', 'localtime') AS INTEGER) < EndHour); ";

            Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "$contentId", contentId }
                };

            byte blockedDays = (byte)SqlSelectInteger(query, args);

            DateTime now = DateTime.Now;
            DayOfWeek dayOfWeek = now.DayOfWeek;
            if (IsHolyday(now)) dayOfWeek = DayOfWeek.Sunday; //Feiertage sind wie Sonntage

            return ((byte)dayOfWeek & blockedDays) > 0; // Ist das Bit dayOfWeek im byte blockedDays vorhanden?
        }

        /// <summary>
        /// Listet die Telefonnummern der aktuellen SMS-Empfänger (Bereitschaft) auf.
        /// Wenn für den aktuellen Tag keine Bereitschaft eingerichtet ist, wird das Bereitschaftshandy eingesetzt.
        /// </summary>
        /// <param name="StandardWatchName">Empfänger, wenn aktuell keine Bereitschaft eingerichtet ist.</param>
        /// <returns>Liste der Telefonnummern derer, die zum aktuellen Zeitpunkt per SMS benachrichtigt werden sollen.</returns>
        public List<ulong> GetCurrentShiftPhoneNumbers(string StandardWatchName = "Bereitschaftshandy")
        {
            try
            {
                #region Stelle sicher, dass es jetzt eine eine aktive Schicht gibt.
                const string query1 = @"SELECT Id " +
                                       "FROM Shifts " +
                                       "WHERE  " +
                                       "CURRENT_TIMESTAMP BETWEEN StartTime AND EndTime ";

                int currentShiftId = SqlSelectInteger(query1);

                if (currentShiftId == 0)
                {
                    //Neue Bereitschaft für Standardempfänger erstellen
                    int contactIdBereitschafshandy = GetContactId(StandardWatchName);
                    if (contactIdBereitschafshandy == 0)
                    {
                        //throw new Exception(" GetCurrentShiftPhoneNumbers(): Kein Kontakt '" + StandardWatchName + "' gefunden.");
                        Program.Sql.Log(LogTopic.Shift, LogPrio.Error, "GetCurrentShiftPhoneNumbers(): Kein Kontakt '" + StandardWatchName + "' in DB gefunden.");
                    }
                    else
                    {
                        //Erzeuge eine neue Schicht für heute mit Standardwerten (Bereitschaftshandy)
                        InsertShift(contactIdBereitschafshandy, DateTime.Now);
                    }
                }
          
                #endregion

                #region Lese Telefonnummern der laufenden Schicht aus der Datenbank
                
                List<ulong> watch = new List<ulong>();

                const string query2 = "SELECT \"Phone\" FROM Contact " +
                                      "WHERE \"SendSms\" > 0 AND " +
                                      "\"Id\" IN " +
                                      "( SELECT ContactId FROM Shifts WHERE CURRENT_TIMESTAMP BETWEEN DateTime(StartTime) AND DateTime(EndTime) )";
                               
                watch = SqlSelectPhoneNumbers(query2);

                if (watch.Count == 0)
                {
                    // throw new Exception(" GetCurrentShiftPhoneNumbers(): Es ist aktuell keine SMS-Bereitschaft definiert."); 

                    //Sollte nur passieren, wenn für das Bereitschaftshandy in DB kein SMS-Versand freigegeben ist.
                    watch.Add(Properties.Settings.Default.MelBoxAdminPhone);
                }
                   

                return watch;
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(" GetCurrentShiftPhoneNumbers()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Listet die Email-Empfänger der aktuellen Bereitschaft auf, sofern vorhanden.
        /// Erzeugt keine neue Bereitschaft. 
        /// </summary>
        /// <returns></returns>
        public System.Net.Mail.MailAddressCollection GetCurrentShiftEmail()
        {

            try
            {
                System.Net.Mail.MailAddressCollection watch = new System.Net.Mail.MailAddressCollection();

                const string query = "SELECT \"Name\", \"Email\" FROM Contact " +
                                     "WHERE \"SendEmail\" > 0 AND " +
                                     "\"Id\" IN " +
                                     "( SELECT ContactId FROM Shifts WHERE CURRENT_TIMESTAMP BETWEEN DateTime(StartTime) AND DateTime(EndTime) )";

                return SqlSelectEmailAddresses(query);
            }
            catch (Exception ex)
            {
                throw new Exception("GetCurrentShiftEmail() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        #endregion

        #region Views

        public DataTable GetViewLog(DateTime StartTime, DateTime EndTime)
        {

            if (StartTime.CompareTo(EndTime) > 0) // > 0; t1 ist später als oder gleich t2.
            {
                DateTime rem = StartTime;
                StartTime = EndTime;
                EndTime = rem;
            }
            else if (StartTime.CompareTo(EndTime) == 0)
            {
                EndTime = EndTime.AddDays(1);
            }

            //CREATE TABLE "Log"("Id" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,"LogTime" TEXT NOT NULL, "Topic" TEXT , "Prio" INTEGER NOT NULL, "Content" TEXT)
            string query = "SELECT Id, datetime(LogTime, 'localtime') AS Zeit, Topic AS Bereich, Prio, Content AS Inhalt FROM \"Log\" " +
                           "WHERE LogTime BETWEEN @startTime AND @endTime ORDER BY Id DESC LIMIT 1000";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@startTime", SqlTime(StartTime) },
                { "@endTime", SqlTime(EndTime) } 
            };

            return SqlSelectDataTable("Logbuch", query, args);
        }
        public DataTable GetViewMsgRec()
        {
            string query = "SELECT * FROM \"ViewMessagesRecieved\" ORDER BY Nr DESC LIMIT 1000";

            return SqlSelectDataTable("Empfangen", query);
        }

        public DataTable GetViewMsgSent()
        {
            string query = "SELECT * FROM \"ViewMessagesSent\" ORDER BY Gesendet DESC LIMIT 1000 ";

            return SqlSelectDataTable("Gesendet", query);
        }

        public DataTable GetViewMsgOverdue()
        {
            string query = "SELECT * FROM \"ViewMessagesOverdue\" GROUP BY Id LIMIT 1000";

            return SqlSelectDataTable("Überfällige Meldungen", query);
        }

        public DataTable GetViewShift(int shiftId)
        {
            string query = "SELECT * FROM \"ViewShift\" WHERE Nr = @id";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@id", shiftId }
            };

            return SqlSelectDataTable("Bereitschaft " + shiftId, query, args);
        }

        public DataTable GetViewShift()
        {
            string query = "SELECT * FROM \"ViewShift\" ";

            //try
            //{
                return SqlSelectDataTable("Bereitschaft", query);
            //}
            //catch 
            //{
            //    DataTable table = new DataTable("LEER - GetViewShift() SQL Abfragefehler");
            //    table.Columns.Add("Fehler", typeof(string));
            //    table.Columns.Add("Hinweis", typeof(string));

            //    //table.Rows.Add(ex.GetType().Name, ex.Message);
            //    return table;
            //}

        }

        public DataTable GetViewMsgBlocked()
        {
            string query = "SELECT * FROM \"ViewMessagesBlocked\"";

            return SqlSelectDataTable("Gesperrte Meldungen", query);
        }

        public DataTable GetViewContactInfo(int contactId = 0)
        {
            string query = "SELECT Contact.Id AS ContactId, Contact.Name AS Name, '********' AS Passwort, CompanyId, Company.Name AS Firma, Phone AS Telefon, Contact.SendSms AS SendSms, Email, Contact.SendEmail AS SendEmail, MaxInactiveHours AS Max_Inaktiv " +
                           "FROM \"Contact\" JOIN \"Company\" ON CompanyId = Company.Id ";

            Dictionary<string, object> args = new Dictionary<string, object>();
           
            if (contactId != 0)
            {
                query += "WHERE Contact.Id = @id ";

                args.Add("@id", contactId);
            }

            return SqlSelectDataTable("Benutzerkonto", query, args);
        }

        /// <summary>
        /// Liest den Datensatz der Firma mitd er übergebenen Id. Id = 0 liest alle Firmen.
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public DataTable GetCompany(int companyId = 0)
        {
            string query = "SELECT Id, Name, Address AS Adresse, City AS Ort FROM \"Company\" ";

            Dictionary<string, object> args = new Dictionary<string, object>();

            if (companyId != 0)
            {
                query += "WHERE Id = @id ";
               
                args.Add("@id", companyId);
            }    

            return SqlSelectDataTable("Firmen", query, args);
        }


        #endregion
    }
}

