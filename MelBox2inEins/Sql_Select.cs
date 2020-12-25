﻿using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxSql
    {
        public int GetMessageId(string content)
        {
            try
            {
                int id = 0;

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    //Neuen Eintrag erstellen, wenn er nicht existiert
                    var command1 = connection.CreateCommand();
                    command1.CommandText = @"
                                INSERT OR IGNORE INTO MessageContent (Content)
                                VALUES ($Content);   
                                SELECT ID FROM MessageContent WHERE Content = $Content
                                ";

                    command1.Parameters.AddWithValue("$Content", content);//.Size = 160; //Max. 160 Zeichen (oder Bytes?)


                    using (var reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (!int.TryParse(reader.GetString(0), out id))
                            {
                                id = GetMessageId(content);
                            }
                        }
                    }
                }

                if (id == 0)
                    //Provisorisch:
                    throw new Exception("GetMessageId() Kontakt konnte nicht zugeordnet werden.");

                return id;
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler GetMessageId()" + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Versucht den Kontakt anhand der Telefonnummer, email-Adresse oder dem Beginn eriner Nachricht zu identifizieren
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
                        while (reader.Read())
                        {
                            //Lese Eintrag
                            if (!int.TryParse(reader.GetString(0), out id))
                            {
                                //Neuen Eintrag erstellen
                                InsertContact(name, null, 1, email, phone, 0, false, false);

                                //Neu erstellten Eintrag lesen
                                id = GetContactId(name, phone, email, message);
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
                                            "CURRENT_TIMESTAMP BETWEEN DateTime(StartDate, '+'||StartHour||' hours') AND DateTime(StartDate, '+1 day', '+'||EndHour||' hours')";

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

    }
}
