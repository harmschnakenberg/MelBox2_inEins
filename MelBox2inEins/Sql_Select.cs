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
        /// <summary>
        /// Ermittelt die ID für den Text der Nachricht. Erstellt für neuen Text eine ID.
        /// </summary>
        /// <param name="content">Nachrichtentext</param>
        /// <returns>ID für den übergebenen content</returns>
        public int GetContentId(string content)
        {
            try
            {
                int id = 0;
                if (content == null) content = "-KEIN TEXT-";

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    //Neuen Eintrag erstellen, wenn er nicht existiert
                    var command1 = connection.CreateCommand();                 
                    //Vermeide ein UNIQUE Constrain in Tabelle MessageContent, um später Abfragen Konfilikte zu vermeiden
                    command1.CommandText = @"INSERT INTO MessageContent(Content) SELECT $Content WHERE NOT EXISTS(SELECT 1 FROM MessageContent WHERE Content = $Content); " +
                                            "SELECT ID FROM MessageContent WHERE Content = $Content; ";

                    command1.Parameters.AddWithValue("$Content", content);

                    using (var reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out id))
                            {
                                return id;
                            }
                        }
                    }
                }

                if (id == 0)
                {
                    //Provisorisch:
                    throw new Exception("GetMessageId(content) Nachricht konnte nicht zugeordnet werden.");
                }

                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetMessageId(content)" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public int GetContentId(int recMsgId)
        {
            try
            {
                int contentId = 0;

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    //Neuen Eintrag erstellen, wenn er nicht existiert
                    var command1 = connection.CreateCommand();
                    command1.CommandText = @"SELECT ContentId FROM LogRecieved WHERE Id = $recMsgid; ";

                    command1.Parameters.AddWithValue("$recMsgid", recMsgId);

                    using (var reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (int.TryParse(reader.GetString(0), out contentId))
                            {
                                return contentId;
                            }
                        }
                    }
                }

                if (contentId == 0)
                {
                    //Provisorisch:
                    throw new Exception("GetContentId() Empfangene Nachricht konnte nicht zugeordnet werden.");
                }

                return contentId;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetContentId(recMsgId)" + ex.GetType() + "\r\n" + ex.Message);
            }
        }


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
            int id = 0;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    string keyWords = ExtractKeyWords(message);

                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT Id " +
                                            "FROM Contact " +
                                            "WHERE  " +
                                            "( length(Name) > 0 AND Name = @name ) " +
                                            "OR ( Phone > 0 AND Phone = @phone ) " +
                                            "OR ( length(Email) > 0 AND Email = @email ) " +
                                            "OR ( length(KeyWord) > 0 AND KeyWord = @keyWord ) ";

                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@phone", phone);
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@message", message);
                    command.Parameters.AddWithValue("@keyWord", keyWords);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            //Neuen Eintrag erstellen
                            id = InsertContact(name, null, 1, email, phone, 0, false, false);
                        }
                        else
                        {
                            int n = 0;
                            //Lese Eintrag
                            while (reader.Read())
                            {
                                n++;
                                int.TryParse(reader.GetString(0), out id);
                            }

                            if (n > 1)
                            {
                                Log(LogTopic.Sql, LogPrio.Warning,
                                    string.Format("Der Kontakt mit der Id {0} ist nicht eindeutig. {1} mögliche Treffer für Name '{2}', Tel.'+{3}', Email:'{4}', KeyWord:'{5}'",
                                    id, n, name, phone, email, keyWords) );
                            }
                        }
                        
                    }
                }

                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetContactId()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public int GetContactIdFromLogin(string name, string password)
        {

            using (var connection = new SqliteConnection(DataSource))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT \"Id\" FROM \"Contact\" WHERE Name = @name AND ( Password = @password OR Password IS NULL )";

                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@password",Encrypt(password));

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows) return 0; // Niemanden mit diesem Namen und diesem Passwort gefunden

                    while (reader.Read())
                    {
                        //Ist die Nachricht zum jetzigen Zeitpunt geblockt?
                        if (!int.TryParse(reader.GetString(0), out int LogedInId)) return 0;

                        return LogedInId;
                    }
                }
            }
            return 0;
        }

        public int GetLastCompany()
        {
            int id = 0;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();
                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT Id " +
                                           "FROM \"Company\" ORDER BY Id DESC LIMIT 1; ";

                    using (var reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int.TryParse(reader.GetString(0), out id);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetLastCompany() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return id;
        }


        public bool IsMessageBlockedNow(string Message)
        {
            int contentId = GetContentId(Message);

            if (contentId == 0) return false;

            using (var connection = new SqliteConnection(DataSource))
            {
                connection.Open();

                //Finde Blockierte Nachricht anhand der ContentId und der Tageszeit
                var command1 = connection.CreateCommand();
                command1.CommandText = @"SELECT Days FROM BlockedMessages " +
                                        "WHERE Id = $contentId AND " +
                                        "(StartHour = EndHour OR " + 
                                        "CAST(strftime('%H','now', 'localtime') AS INTEGER) > StartHour OR "+
                                        "CAST(strftime('%H','now', 'localtime') AS INTEGER) < EndHour); ";

                command1.Parameters.AddWithValue("$contentId", contentId);

                using (var reader = command1.ExecuteReader())
                {
                    if (!reader.HasRows) return false;

                    while (reader.Read())
                    {
                        //Lese Eintrag
                        if (byte.TryParse(reader.GetString(0), out byte blockedDays))
                        {
                            DateTime now = DateTime.Now;
                            DayOfWeek dayOfWeek = now.DayOfWeek;
                            if (IsHolyday(now)) dayOfWeek = DayOfWeek.Sunday; //Feiertage sind wie Sonntage

                           return ((byte)dayOfWeek & blockedDays) > 0; // Ist das Bit dayOfWeek im byte blockedDays vorhanden?
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Listet die Telefonnummern der aktuellen SMS-Empfänger (Bereitschaft) auf.
        /// Wenn für den aktuellen Tag keine Bereitschaft eingerichtet ist, wird das Bereitschaftshandy eingesetzt.
        /// </summary>
        /// <returns>Liste der Telefonnummern derer, die zum aktuellen Zeitpunkt per SMS benachrichtigt werden sollen.</returns>
        public List<ulong> GetCurrentShiftPhoneNumbers()
        {
            const string StandardWatchName = "Bereitschaftshandy";

            try
            {
                List<ulong> watch = new List<ulong>();

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    #region Stelle sicher, dass es jetzt eine eine aktive Schicht gibt.
                    command.CommandText = @"SELECT Id " +
                                            "FROM Shifts " +
                                            "WHERE  " +
                                            "CURRENT_TIMESTAMP BETWEEN StartTime AND EndTime ";

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            int contactIdBereitschafshandy = GetContactId(StandardWatchName);
                            if (contactIdBereitschafshandy == 0)
                            {
                                throw new Exception(" GetCurrentShiftPhoneNumbers(): Kein Kontakt '" + StandardWatchName + "' gefunden.");
                            }

                            //Erzeuge eine neue Schicht für heute mit Standardwerten (Bereitschaftshandy)
                            InsertShift(contactIdBereitschafshandy);
                        }

                    }
                    #endregion

                    #region Lese Telefonnummern der laufenden Schicht aus der Datenbank
                    command.CommandText = "SELECT \"Phone\" FROM Contact " +
                                            "WHERE \"SendSms\" > 0 AND " +
                                            "\"Id\" IN " +
                                            "( SELECT ContactId FROM Shifts WHERE CURRENT_TIMESTAMP BETWEEN DateTime(StartTime) AND DateTime(EndTime) )";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (ulong.TryParse(reader.GetString(0), out ulong phone))
                            {
                                watch.Add(phone);
                            }
                        }
                    }
                    #endregion
                }

                if (watch.Count == 0)
                    throw new Exception(" GetCurrentShiftPhoneNumbers(): Es ist aktuell keine SMS-Bereitschaft definiert."); //Exception? Muss anderweitig abgefangen werden.

                return watch;
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

                foreach (string mail in Email.PermanentEmailRecievers)
                {
                    watch.Add(new System.Net.Mail.MailAddress(mail));
                }


                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    #region Lese Emailadressen der laufenden Schicht aus der Datenbank
                    command.CommandText = "SELECT \"Name\", \"Email\" FROM Contact " +
                                            "WHERE \"SendEmail\" > 0 AND " +
                                            "\"Id\" IN " +
                                            "( SELECT ContactId FROM Shifts WHERE CURRENT_TIMESTAMP BETWEEN DateTime(StartTime) AND DateTime(EndTime) )";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            string name = reader.GetString(0);
                            string mail = reader.GetString(1);

                            if ( IsEmail(mail) )
                            {
                                watch.Add(new System.Net.Mail.MailAddress(mail, name));
                            }
                        }
                    }
                    #endregion
                }


                return watch;
            }
            catch (Exception ex)
            {
                throw new Exception("GetCurrentShiftEmail() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }


        #region Views
        public DataTable GetViewLog(DateTime StartTime, DateTime EndTime)
        {

            if (StartTime.CompareTo(EndTime) >= 0) // > 0; t1 ist später als oder gleich t2.
            {
                StartTime = EndTime.AddDays(-3);
            }

            DataTable recTable = new DataTable
            {
                TableName = "Logbuch"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"Log\" WHERE LogTime BETWEEN @startTime AND @endTime ORDER BY Id DESC LIMIT 1000";

                    command1.Parameters.AddWithValue("@startTime", SqlTime(StartTime));
                    command1.Parameters.AddWithValue("@endTime", SqlTime(EndTime));

                    using (var reader = command1.ExecuteReader())
                    {
                        recTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewLog() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return recTable;
        }

        public DataTable GetViewMsgRec()
        {
            DataTable recTable = new DataTable
            {
                TableName = "Empfangen",               
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewMessagesRecieved\" ORDER BY Nr DESC LIMIT 1000";

                    using (var reader = command1.ExecuteReader())
                    {
                        recTable.Load(reader);                       
                    }
                }
            }
            catch (Exception ex)
            {
                Log(LogTopic.Sql, LogPrio.Error, "Sql - Fehler GetViewMsgRec() " + ex.GetType() + "\r\n" + ex.Message);
                throw new Exception("Sql-Fehler GetViewMsgRec() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return recTable;
        }

        public DataTable GetViewMsgSent()
        {
            DataTable sentTable = new DataTable
            {
                TableName = "Gesendet"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewMessagesSent\" ORDER BY Gesendet DESC LIMIT 1000 ";

                    using (var reader = command1.ExecuteReader())
                    {
                        sentTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewMsgSent() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return sentTable;
        }

        public DataTable GetViewMsgOverdue()
        {
            DataTable overdueTable = new DataTable
            {
                TableName = "Überfällige Meldungen"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewMessagesOverdue\" ORDER BY Fällig_seit DESC LIMIT 1000";

                    using (var reader = command1.ExecuteReader())
                    {
                        overdueTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewMsgOverdue() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return overdueTable;
        }

        public DataTable GetViewShift(int shiftId)
        {
            DataTable shiftTable = new DataTable
            {
                TableName = "Bereitschaft " + shiftId
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewShift\" WHERE Nr = $id";
                    command1.Parameters.AddWithValue("$id", shiftId);

                    using (var reader = command1.ExecuteReader())
                    {
                        shiftTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewShift(shiftId) " + ex.GetType() + "\r\n" + ex.Message);
            }

            return shiftTable;
        }

        public DataTable GetViewShift()
        {
            DataTable shiftTable = new DataTable
            {
                TableName = "Bereitschaft"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewShift\" ORDER BY Datum LIMIT 1000";

                    using (var reader = command1.ExecuteReader())
                    {
                        shiftTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewShift() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return shiftTable;
        }

        public DataTable GetViewMsgBlocked()
        {
            DataTable overdueTable = new DataTable
            {
                TableName = "Gesperrte Meldungen"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT * FROM \"ViewMessagesBlocked\"";

                    using (var reader = command1.ExecuteReader())
                    {
                        overdueTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewMsgBlocked() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return overdueTable;
        }

        public DataTable GetViewContactInfo(int contactId)
        {
            DataTable contactTable = new DataTable
            {
                TableName = "Benutzerkonto"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT Contact.Id AS ContactId, Contact.Name AS Name, '********' AS Passwort, CompanyId, Company.Name AS Firma, Email, Phone AS Telefon, Contact.SendSms AS SendSms , Contact.SendEmail AS SendEmail, MaxInactiveHours AS Max_Inaktivität " +
                        "FROM \"Contact\" JOIN \"Company\" ON CompanyId = Company.Id WHERE Contact.Id = @id; ";

                    command1.Parameters.AddWithValue("@id", contactId);

                    using (var reader = command1.ExecuteReader())
                    {
                        contactTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetViewContactInfo() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return contactTable;
        }

        public DataTable GetAllCompanys()
        {
            DataTable companyTable = new DataTable
            {
                TableName = "Firmen"
            };

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();


                    var command1 = connection.CreateCommand();

                    command1.CommandText = "SELECT Id, Name, Address AS Adresse, City AS Ort " +
                                           "FROM \"Company\" ";

                    using (var reader = command1.ExecuteReader())
                    {
                        companyTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetAllCompanys() " + ex.GetType() + "\r\n" + ex.Message);
            }

            return companyTable;
        }

        #endregion
    }
}
