using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxSql
    {
        #region enums
        /// <summary>
        /// Kategorien für Logging
        /// </summary>
        public enum LogTopic
        {
            Allgemein,
            Start,
            Shutdown,
            Sms,
            Email,
            Sql,
            Shift
        }

        /// <summary>
        /// Priorisierung von Log-EInträgen (ggf später auch Meldungen )
        /// </summary>
        public enum LogPrio
        {
            Unknown,
            Error,
            Warning,
            Info
        }

        public enum SendStatus
        {
            OnlyDb,
            SetToSent,
            Pending,
            SendAgain,
            SendAbborted,
            SentSuccess
        }

        public enum SendWay
        {
            Unknown,
            Sms,
            Email            
        }

        /// <summary>
        /// Bit-Codierung, an welchen Wochentagen eine Störung gesperrt sein soll. Feiertage zählen als Sonntage.
        /// Alle Tage = 1 + 2 + 4 + 8 + 16 + 32 + 64 = 127
        /// Nur Werktage = 2 + 4 + 8 + 16 + 32 = 62
        /// Nur am Wochenende: 1 + 64 = 65 
        /// </summary>
        [Flags]
        public enum BlockedDays : byte
        {
            Sunday = 1,
            Monday = 2,
            Tuesday = 4,
            Wendsday = 8,
            Thursday = 16,
            Friday = 32,
            Saturday = 64,
            Weekdays = 62,
            AllDays = 127
        }

        #endregion

        /// <summary>
        /// Neuer Log-Eintrag
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="prio"></param>
        /// <param name="content"></param>
        public void Log(LogTopic topic, LogPrio prio, string content)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Log(LogTime, Topic, Prio, Content) VALUES (CURRENT_TIMESTAMP, @topic, @prio, @content)";

                    command.Parameters.AddWithValue("@topic", topic.ToString());
                    command.Parameters.AddWithValue("@prio", (ushort)prio);
                    command.Parameters.AddWithValue("@content", content);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler Log() " + ex.GetType().Name);
            }
        }

        /// <summary>
        /// Neuer Eintrag für Unternehmen
        /// </summary>
        /// <param name="name">Anzeigename des Unternehmens</param>
        /// <param name="address">Standortadresse</param>
        /// <param name="city">PLZ, Ort</param>
        public bool InsertCompany(string name, string address, string city)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Company\" (\"Name\", \"Address\", \"City\") VALUES (@name, @address, @city );";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@address", address);
                    command.Parameters.AddWithValue("@city", city);

                    return 0 != command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler InsertCompany()");
            }
        }

        /// <summary>
        /// Neuer Eintrag für Kontakt (Kunde, Bereitschaftsnehmer)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="passwordPlain"></param>
        /// <param name="companyId"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="maxInactiveHours"></param>
        /// <param name="sendSms"></param>
        /// <param name="sendEmail"></param>
        /// <returns>ID des neu eingetragenen Kontakts</returns>
        public int InsertContact(string name, string passwordPlain, int companyId, string email, ulong phone, int maxInactiveHours, bool sendSms, bool sendEmail)
        {
            int id = 0;

            if (name.Length < 3) name = "-KEIN NAME-";
            if (companyId < 1) companyId = 1;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Contact\" (\"Name\", \"Password\", \"CompanyId\", \"Email\", \"Phone\", \"MaxInactiveHours\", \"SendSms\", \"SendEmail\" ) " +
                                                           "VALUES ( @name , @password, @companyId, @email, @phone, @maxinactivehours, @sendSms, @sendEmail); " +
                                                           "SELECT Id FROM \"Contact\" ORDER BY Id DESC LIMIT 1; ";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@password", passwordPlain == null ? (object)DBNull.Value : Encrypt(passwordPlain));
                    command.Parameters.AddWithValue("@companyId", companyId);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@maxinactivehours", maxInactiveHours);
                    command.Parameters.AddWithValue("@sendSms", sendSms ? 1 : 0);
                    command.Parameters.AddWithValue("@sendEmail", sendEmail ? 1 : 0);

                    using (var reader = command.ExecuteReader())
                    {
                        //Lese Eintrag
                        while (reader.Read())
                        {
                            if (int.TryParse(reader.GetString(0), out id))
                                return id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sql-Fehler InsertContact()\r\n" + 
                    ex.GetType().ToString() + ": " + ex.Message + "\r\n" + ex.InnerException);
                throw ex; 
            }
            return id;
        }

        /// <summary>
        /// Schreibt den Empfang einer neuen Nachricht in die Datenbank.
        /// Gibt die ID der Nachricht in der Empfangsliste aus.
        /// FRAGE: Rückgabewert ID aus DB notwendig?
        /// </summary>
        /// <param name="message"></param>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <returns>ID der Empfangenen Nachricht; Wenn nicht erfolgreich 0.</returns>
        public int InsertMessageRec(string message, ulong phone = 0, string email = "")
        {
            //"CREATE TABLE \"LogRecieved\"( \"Id\" , \"RecieveTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"FromContactId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL);",

            int msgId;
            try
            {
                //Absender identifizieren
                int senderId = GetContactId("", phone, email, message);
                //Inhalt identifizieren
                msgId = GetContentId(message);

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"LogRecieved\" (\"FromContactId\", \"ContentId\") VALUES " +
                                          "(@fromContactId, @contentId ); " +
                                          "SELECT Id FROM \"LogRecieved\" ORDER BY \"RecieveTime\" DESC LIMIT 1";

                    command.Parameters.AddWithValue("@fromContactId", senderId);
                    command.Parameters.AddWithValue("@contentId", msgId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out int recId))
                            {
                               // Console.WriteLine("Neue SMS mit Empfangs-Id {0} gespeichert.", recId);
                                return msgId;
                            }
                        }
                    }

                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("InsertRecMessage()" + ex.GetType() + "\r\n" + ex.Message);
            }

        }

        /// <summary>
        /// Protokolliert die Weiterleitung einer Nachricht
        /// </summary>
        /// <param name="contentId">Id für den Inhalt (Text) der Nachricht</param>
        /// <param name="sentToId">Id für den Empfänger der Nachricht</param>
        /// <param name="sendVia">Sendeweg 1 = SMS, 2 = Email</param>
        /// <param name="smsReference">Index aus GSM Modem bei SMS</param>
        /// <param name="confirmStatus">Statuscode 0=Eintrag erstellt, 1=Versendet, 2=Warten auf Sendebericht, 4=Nochmal senden, 8=Senden abgebrochen, 16=senden erfolgreich</param>
        /// <returns></returns>
        public int InsertMessageSent(int contentId, int sentToId, SendWay sendVia, int smsReference, SendStatus confirmStatus = 0)
        {
            // "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"SentToId\", \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, \"SmsIndex\" INTEGER, \"ConfirmStatus\" INTEGER);" +

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"LogSent\" (\"SentToId\", \"ContentId\", \"SentVia\", \"SmsRef\", \"ConfirmStatus\" ) " +
                                          "VALUES (@sentToId, @contentId, @sendVia, @smsRef, @confirmStatus);" +
                                          "SELECT Id FROM \"LogSent\" ORDER BY \"SentTime\" DESC LIMIT 1";

                    command.Parameters.AddWithValue("@sentToId", sentToId);
                    command.Parameters.AddWithValue("@contentId", contentId);
                    command.Parameters.AddWithValue("@sendVia", sendVia);
                    command.Parameters.AddWithValue("@smsRef", smsReference);
                    command.Parameters.AddWithValue("@confirmStatus", confirmStatus);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out int sendId))
                            {
                                return sendId;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sql-Fehler InsertMessageSent()\r\n" +
                    ex.GetType().ToString() + ": " + ex.Message + "\r\n" + ex.InnerException);
                throw ex;
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="phoneTo"></param>
        /// <param name="smsReference"></param>
        /// <param name="smsSendStatus">von Modem: 0..31 success, 32..63 pending, 64..127 aborted; von mir: >127 für intern</param>
        public void InsertMessageSent(string message, ulong phoneTo, int smsReference, byte smsSendStatus = 255)
        {
            int contentId = GetContentId(message);
            int contactId = GetContactId("", phoneTo , "", message);

            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;
            else if (smsSendStatus >= 128)
                sendStatus = SendStatus.SetToSent;

            InsertMessageSent(contentId, contactId, SendWay.Sms, smsReference, sendStatus);
        }

        public int InsertMessageStatus( SendWay sendVia, int smsReference, SendStatus confirmStatus = 0)
        {
            // "CREATE TABLE \"LogSent\"   (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"SentToId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, \"SmsRef\" INTEGER, \"ConfirmStatus\" INTEGER);" +
            // "CREATE TABLE \"LogStatus\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"SentToId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, \"SmsRef\" INTEGER, \"ConfirmStatus\" INTEGER);" +

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"LogStatus\" (\"SentVia\", \"SmsRef\", \"ConfirmStatus\" ) " +
                                          "VALUES ( @sendVia, @smsRef, @confirmStatus);" +
                                          "SELECT Id FROM \"LogStatus\" ORDER BY \"SentTime\" DESC LIMIT 1";

                    command.Parameters.AddWithValue("@sendVia", sendVia);
                    command.Parameters.AddWithValue("@smsRef", smsReference);
                    command.Parameters.AddWithValue("@confirmStatus", confirmStatus);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out int sendId))
                            {
                                return sendId;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("InsertMessageStatus() " + ex.GetType().Name + "\r\n" + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="phoneTo"></param>
        /// <param name="smsReference"></param>
        /// <param name="smsSendStatus">von Modem: 0..31 success, 32..63 pending, 64..127 aborted; von mir: >127 für intern</param>
        public void InsertMessageStatus(int smsReference, byte smsSendStatus = 255)
        {

            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;
            else if (smsSendStatus >= 128)
                sendStatus = SendStatus.SetToSent;

            InsertMessageStatus( SendWay.Sms, smsReference, sendStatus);
        }


        /// <summary>
        /// Fügt eine Nachricht der "Blacklist" hinzu, sodass diese bei Empfang nicht weitergeleitet wird.
        /// Ist die Nachricht bereits in der Liste vorhanden, wird kein neuer Eintrag erstellt.
        /// Standartsperrzeit: immer
        /// </summary>
        /// <param name="msgId">Id der Nachricht, deren Weiterleitung gesperrt werden soll</param>
        /// <param name="startHour">Tagesstunde - Beginn der Sperre</param>
        /// <param name="endHour">Tagesstunde - Ende der Sperre</param>
        /// <param name="Days">Tage, an denen die Nachricht gesperrt sein soll, wie Wochenuhr/param>
        public void InsertMessageBlocked(int msgId, int startHour = 7, int endHour = 7, BlockedDays blockedDays = BlockedDays.AllDays)
        {
            if (blockedDays == 0)
            {
                //Wenn ein neuer Eintrag, dann Standartwert alle Tage 
                blockedDays = BlockedDays.AllDays;
            }

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    //Nur neuen Eintrag erzeugen, wenn msgId noch nicht vorhanden ist.
                    command.CommandText = "INSERT OR IGNORE INTO \"BlockedMessages\" (\"Id\", \"StartHour\", \"EndHour\", \"Days\" ) VALUES " +
                                          "(@msgId, @startHour, @endHour, @days)";

                    command.Parameters.AddWithValue("@msgId", msgId);
                    command.Parameters.AddWithValue("@startHour", startHour);
                    command.Parameters.AddWithValue("@endHour", endHour);
                    command.Parameters.AddWithValue("@days", blockedDays);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler InsertBlockedMessage()\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Erstellt einen neuen Bereitschaftsdienst über einen Tagessprung.
        /// </summary>
        /// <param name="contactId">Id des Bereitschaftsnehmers</param>
        /// <param name="startDate">Beginn der Bereitschaft (lokalzeit)</param>
        /// <param name="endDate">Ende der Bereitschaft (lokalzeit)</param>
        public void InsertShift(int contactId, DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO \"Shifts\" (\"EntryTime\", \"ContactId\", \"StartTime\", \"EndTime\") VALUES " +
                                          "(CURRENT_TIMESTAMP, @contactId, @startTime, @endTime )";

                    command.Parameters.AddWithValue("@contactId", contactId);
                    command.Parameters.AddWithValue("@startTime", SqlTime(startDate.ToUniversalTime()));
                    command.Parameters.AddWithValue("@endTime", SqlTime(endDate.ToUniversalTime()));
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler InsertShift()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Erzeugt eine Bereitschaft für heute bis morgen mit automatisch ermittelter Anfangs- und Endzeit.
        /// </summary>
        /// <param name="contactId">Id des Bereitschaftsnehmers</param>
        public void InsertShift(int contactId)
        {
            DateTime Today = DateTime.Now.Date;
            DateTime EndTime = Today.AddDays(1).AddHours(7);
            DateTime StartTime = Today.AddHours(17);

            if (Today.DayOfWeek == DayOfWeek.Friday)
            {
                StartTime = Today.AddHours(15);
            }

            if (Today.DayOfWeek == DayOfWeek.Saturday || Today.DayOfWeek == DayOfWeek.Sunday || IsHolyday(Today))
            {
                StartTime = Today.AddHours(7);
            }

            InsertShift(contactId, StartTime, EndTime);
        }
    }
}
