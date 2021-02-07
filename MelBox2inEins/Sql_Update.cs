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

        /// <summary>
        /// BAUSTELLE funktioniert noch nicht
        /// </summary>
        /// <param name="sendVia"></param>
        /// <param name="smsReference"></param>
        /// <param name="sendTime"></param>
        /// <param name="smsSendStatus"></param>
        public bool UpdateMessageSentStatus(SendWay sendVia, int smsReference, DateTime sendTime, byte smsSendStatus = 255)
        {
            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;
            try
            {
                const string query = "UPDATE \"LogSent\" " +
                                 "SET \"ConfirmStatus\" = @confirmStatus " +
                                 "WHERE ID = (SELECT ID FROM \"LogSent\" " +
                                 "WHERE \"SmsRef\" = @smsRef " +
                                 "AND \"SendVia\" = @sendVia " +
                                 "AND \"ConfirmStatus\" = @confirmStatusOld " +
                                 "AND \"SentTime\" BETWEEN datetime(@sentTime, '-5 minutes') AND datetime(@sentTime)" +
                                 "ORDER BY \"SentTime\" DESC LIMIT 1); ";

            Dictionary<string, object> args = new Dictionary<string, object>
            {
                { "@sendVia", sendVia },
                { "@smsRef", smsReference },
                { "@confirmStatus", sendStatus },
                { "@confirmStatusOld", SendStatus.SetToSent },
                { "@sentTime", MelBoxSql.SqlTime(sendTime) },
            };

            return SqlNonQuery(query, args);

            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateMessageSent() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Ändere Benutzerinformationen
        /// </summary>
        /// <param name="contactId">Kontakt-Id auf die sich die Änderungen beziehen</param>
        /// <param name="name">Anzeigename</param>
        /// <param name="password">Passwort wird im Klartext übergeben und verschlüsselt gespeichert</param>
        /// <param name="companyId">Id der Firma</param>
        /// <param name="phone">Telefonnumer als Zahl mit Ländervorwahl</param>
        /// <param name="sendSms">0=nein, 1 = ja, -1= nicht ändern</param>
        /// <param name="email"></param>
        /// <param name="sendEmail">0=nein, 1 = ja, -1= nicht ändern</param>
        /// <param name="keyWord">Soll nur aus empfangener SMS ermittelt werden.</param>
        /// <returns>anzahl der betroffenen Zeilen in der Tabelle</returns>
        public bool UpdateContact(int contactId, string name = "", string password = "", int companyId = 0, ulong phone = 0, int sendSms = -1, string email = "", int sendEmail = -1, string keyWord = "", int maxInactivity = -1)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                Dictionary<string, object> args = new Dictionary<string, object>
                { 
                    { "@contactId", contactId }
                };

                if (name.Length > 3)
                    {
                        args.Add("@name", name);
                        builder.Append("UPDATE \"Contact\" SET Name = @name WHERE Id = @contactId; ");
                    }

                    if (password != null && password.Length > 3)
                    {
                        Console.WriteLine("Passwort\t" + password + "=" + Encrypt(password));
                        args.Add("@password", Encrypt(password));
                        builder.Append("UPDATE \"Contact\" SET Password = @password WHERE Id = @contactId; ");
                    }

                    if (companyId > 0)
                    {
                        args.Add("@companyId", companyId);
                        builder.Append("UPDATE \"Contact\" SET CompanyId = @companyId WHERE Id = @contactId; ");
                    }

                    if (phone > 0)
                    {
                        args.Add("@phone", phone);
                        builder.Append("UPDATE \"Contact\" SET Phone = @phone WHERE Id = @contactId; ");
                    }

                    if (sendSms > -1)
                    {
                        args.Add("@sendSms", sendSms);
                        builder.Append("UPDATE \"Contact\" SET SendSms = @sendSms WHERE Id = @contactId; ");
                    }


                    if (IsEmail(email))
                    {
                        args.Add("@email", email);
                        builder.Append("UPDATE \"Contact\" SET Email = @email WHERE Id = @contactId; ");
                    }

                    if (sendEmail > -1)
                    {
                        args.Add("@sendEmail", sendEmail);
                        builder.Append("UPDATE \"Contact\" SET SendEmail = @sendEmail WHERE Id = @contactId; ");
                    }

                    if (keyWord.Length > 3)
                    {
                        args.Add("@keyWord", keyWord);
                        builder.Append("UPDATE \"Contact\" SET KeyWord = @keyWord WHERE Id = @contactId; ");
                    }

                    if (maxInactivity > -1)
                    {
                        args.Add("@maxInactivity", maxInactivity);
                        builder.Append("UPDATE \"Contact\" SET MaxInactiveHours = @maxInactivity WHERE Id = @contactId; ");
                    }


                return SqlNonQuery(builder.ToString(), args);                
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public bool UpdateCompany(int companyId, string name = "", string address = "", string city = "")
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    StringBuilder builder = new StringBuilder();

                    connection.Open();
                    var command = connection.CreateCommand();
                    command.Parameters.AddWithValue("@companyId", companyId);

                    if (name.Length > 3)
                    {
                        command.Parameters.AddWithValue("@name", name);
                        builder.Append("UPDATE \"Company\" SET Name = @name WHERE Id = @companyId; ");
                    }

                    if (address.Length > 3)
                    {
                        command.Parameters.AddWithValue("@address", address);
                        builder.Append("UPDATE \"Company\" SET Address = @address WHERE Id = @companyId; ");
                    }

                    if (city.Length > 3)
                    {
                        command.Parameters.AddWithValue("@city", city);
                        builder.Append("UPDATE \"Company\" SET City = @city WHERE Id = @companyId; ");
                    }

                    command.CommandText = builder.ToString();
                    return 0 != command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public bool UpdateMessageBlocked(int msgId, int startHour = 7, int endHour = 7, BlockedDays blockedDays = BlockedDays.AllDays)
        {
            try
            {
                const string query = "UPDATE \"BlockedMessages\" SET \"StartHour\" = @startHour, \"EndHour\" = @endHour, \"Days\" = @days " +
                                     "WHERE \"Id\" = @msgId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@msgId", msgId },
                    { "@startHour", startHour },
                    { "@endHour", endHour },
                    { "@days", blockedDays }
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler Update BlockedMessage()\r\n" + ex.Message);
            }
        }

        public bool UpdateShift(int shiftId, int contactId, DateTime startDate, DateTime endDate)
        {
            try
            {
                const string query = "UPDATE \"Shifts\" SET \"EntryTime\" = CURRENT_TIMESTAMP, \"ContactId\" = @contactId, \"StartTime\" = @startTime, \"EndTime\" = @endTime " +
                                     "WHERE \"Id\" = @shiftId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@shiftId", shiftId },
                    { "@contactId", contactId },
                    { "@startTime", SqlTime(startDate.ToUniversalTime()) },
                    { "@endTime", SqlTime(endDate.ToUniversalTime()) }
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateShift()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }
    }
}

/*
 * MAGAZIN
 * 
   public void UpdateMessageSentStatus(SendWay sendVia, int smsReference, DateTime sendTime, byte smsSendStatus = 255)
        {
            // "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
            //\"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
            //\"SentToId\", \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, 
            //\"SmsIndex\" INTEGER, \"ConfirmStatus\" INTEGER);" +

            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE \"LogSent\" " +
                          "SET \"ConfirmStatus\" = @confirmStatus " +
                          "WHERE ID = (SELECT ID FROM \"LogSent\" " +
                          "WHERE \"SmsRef\" = @smsRef " +
                          "AND \"SendVia\" = @sendVia " +
                          "AND \"ConfirmStatus\" = @confirmStatusOld " +
                          "AND \"SentTime\" BETWEEN datetime(@sentTime, '-5 minutes') AND datetime(@sentTime)" +
                          "ORDER BY \"SentTime\" DESC LIMIT 1); ";

                    //command.Parameters.AddWithValue("@sentToId", sentToId);
                    //command.Parameters.AddWithValue("@contentId", contentId);
                    command.Parameters.AddWithValue("@sendVia", sendVia);
                    command.Parameters.AddWithValue("@smsRef", smsReference);
                    command.Parameters.AddWithValue("@confirmStatus", sendStatus );
                    command.Parameters.AddWithValue("@confirmStatusOld", SendStatus.OnlyDb);
                    command.Parameters.AddWithValue("@sentTime", MelBoxSql.SqlTime(sendTime) );

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateMessageSent() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

     public int UpdateContact(int contactId, string name = "", string password = "", int companyId = 0, ulong phone = 0, int sendSms = -1, string email = "", int sendEmail = -1, string keyWord = "", int maxInactivity = -1)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.Parameters.AddWithValue("@contactId", contactId);

                    if (name.Length > 3)
                    {
                        command.Parameters.AddWithValue("@name", name);
                        builder.Append("UPDATE \"Contact\" SET Name = @name WHERE Id = @contactId; ");
                    }

                    if (password.Length > 3)
                    {
                        Console.WriteLine("Passwort\t" + password + "=" + Encrypt(password));
                        command.Parameters.AddWithValue("@password", Encrypt(password));
                        builder.Append("UPDATE \"Contact\" SET Password = @password WHERE Id = @contactId; ");
                    }

                    if (companyId > 0)
                    {
                        command.Parameters.AddWithValue("@companyId", companyId);
                        builder.Append("UPDATE \"Contact\" SET CompanyId = @companyId WHERE Id = @contactId; ");
                    }

                    if (phone > 0)
                    {
                        command.Parameters.AddWithValue("@phone", phone);
                        builder.Append("UPDATE \"Contact\" SET Phone = @phone WHERE Id = @contactId; ");
                    }

                    if (sendSms > -1)
                    {
                        command.Parameters.AddWithValue("@sendSms", sendSms);
                        builder.Append("UPDATE \"Contact\" SET SendSms = @sendSms WHERE Id = @contactId; ");
                    }


                    if (IsEmail(email))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        builder.Append("UPDATE \"Contact\" SET Email = @email WHERE Id = @contactId; ");
                    }

                    if (sendEmail > -1)
                    {
                        command.Parameters.AddWithValue("@sendEmail", sendEmail);
                        builder.Append("UPDATE \"Contact\" SET SendEmail = @sendEmail WHERE Id = @contactId; ");
                    }

                    if (keyWord.Length > 3)
                    {
                        command.Parameters.AddWithValue("@keyWord", keyWord);
                        builder.Append("UPDATE \"Contact\" SET KeyWord = @keyWord WHERE Id = @contactId; ");
                    }

                    if (maxInactivity > -1)
                    {
                        command.Parameters.AddWithValue("@maxInactivity", maxInactivity);
                        builder.Append("UPDATE \"Contact\" SET MaxInactiveHours = @maxInactivity WHERE Id = @contactId; ");
                    }

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = builder.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

      public int UpdateCompany(int companyId, string name = "", string address = "", string city = "")
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    StringBuilder builder = new StringBuilder();

                    connection.Open();
                    var command = connection.CreateCommand();
                    command.Parameters.AddWithValue("@companyId", companyId);

                    if (name.Length > 3)
                    {
                        command.Parameters.AddWithValue("@name", name);
                        builder.Append("UPDATE \"Company\" SET Name = @name WHERE Id = @companyId; ");
                    }

                    if (address.Length > 3)
                    {
                        command.Parameters.AddWithValue("@address", address);
                        builder.Append("UPDATE \"Company\" SET Address = @address WHERE Id = @companyId; ");
                    }

                    if (city.Length > 3)
                    {
                        command.Parameters.AddWithValue("@city", city);
                        builder.Append("UPDATE \"Company\" SET City = @city WHERE Id = @companyId; ");
                    }

#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = builder.ToString();
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }


//*/