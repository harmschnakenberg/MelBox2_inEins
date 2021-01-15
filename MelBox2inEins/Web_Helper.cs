using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxWeb
    {
        public static string EncodeUmlaute(string input)
        {
            //&     &amp;   Zerstört Sonderzeichen in HTML!
            //"     &quot;
            //<     &lt;
            //>     &gt;
            //' 	&apos;
            return input.Replace("Ä", "&Auml;").Replace("Ö", "&Ouml;").Replace("Ü", "&Uuml;").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;").Replace("ß", "&szlig;");
        }

        public static string DecodeUmlaute(string input)
        {
            return input.Replace("%C4", "Ä").Replace("%D6", "Ö").Replace("%DC", "Ü").Replace("%E4", "ä").Replace("%F6", "ö").Replace("%FC", "ü")
                .Replace("%DF", "ß").Replace("%40", "@").Replace("%2B", "+").Replace("%26", "&").Replace("%22", "\"");
        }

        public static Dictionary<string, string> ReadPayload(string payload)
        {
            payload = payload ?? string.Empty;
            Dictionary<string, string> dict = new Dictionary<string, string>();

            payload = DecodeUmlaute(payload);

            string[] args = payload.Split('&');

            foreach (string arg in args)
            {
                string[] items = arg.Split('=');

                if (items.Length > 1)
                    dict.Add(items[0], items[1]);
            }

            return dict;
        }


        private static Dictionary<string, User> LogedInUsers { get; set; } = new Dictionary<string, User>();

        public static string CheckLogin(string name, string password)
        {
            //Benuterdaten aus DB laden, wenn Name + Password korrekt
            DataTable dtAccount = Program.Sql.GetContactFromLogin(name, password);

            if (dtAccount.Rows.Count == 0) return string.Empty;

            //Neue Instanz von User erstellen und in die Liste einfügen
            User user = new User();

            foreach (DataRow r in dtAccount.Rows)
            {
                foreach (DataColumn c in dtAccount.Columns)
                {
                    switch (c.ColumnName)
                    {
                        //ContactId,  Name, Passwort, CompanyId, Firma, Email, Telefon, SendSms, SendEmail, Max_Inaktivität 

                        case "Id":
                            int.TryParse(r[c.ColumnName].ToString(), out int id);
                            user.Id = id;
                            user.IsAdmin = MelBoxSql.AdminIds.Contains(id);
                            break;

                        case "Name":
                            user.Name = r[c.ColumnName].ToString();
                            break;
                    }
                }
            }

            if (user.Id == 0) return string.Empty;

            string guid = Guid.NewGuid().ToString("N");
            LogedInUsers.Add(guid, user);

//#if DEBUG
//            Console.WriteLine("Angemeldet:");
//            foreach (string item in LogedInUsers.Keys)
//            {
//                User x = LogedInUsers[item];

//                Console.WriteLine("{0}\t{1}\t{2}", x.IsAdmin ? "Admin" : "Benutzer", x.Id, x.Name);
//            }
//#endif
            return guid;
        }

        public static User GetUserFromGuid(string guid)
        {
            if (LogedInUsers.ContainsKey(guid))
                return LogedInUsers[guid];
            else
                return null;
        }

        /// <summary>
        /// Verarbeitet die Anfrage aus dem Formular 'Benutzerkonto'
        /// </summary>
        /// <param name="args"></param>
        /// <param name="createNewAccount">Soll ein neuer Benutzer erstellt werden?</param>
        /// <returns>html - Rückmeldung der ausgeführten Operation</returns>
        public static string ProcessFormAccount(Dictionary<string, string> args, bool createNewAccount)
        {
            StringBuilder builder = new StringBuilder();

            int contactId = 0;
            string name = "-KEIN NAME-";
            string password = null;
            int companyId = -1;
            string email = null;
            ulong phone = 0;
            int sendSms = 0; //Hinweis: <input type='checkbox' > wird nur übertragen, wenn angehakt => immer zurücksetzten, wenn nicht gesetzt
            int sendEmail = 0;
            int maxInactivity = -1;

            foreach (string arg in args.Keys)
            {
                switch (arg)
                {
                    case "pageTitle":
                        if (args[arg] != "Benutzerkonto")
                        {
                            builder.Append(MelBoxWeb.HtmlAlert(1, "Ungültiger Aufruf", "Aufruf von ungültiger Stelle."));
                        }
                        break;
                    case "ContactId":
                        contactId = int.Parse(args[arg]);
                        break;
                    case "Name":
                        name = DecodeUmlaute( args[arg].Replace('+', ' ') );
                        break;
                    case "Passwort":
                        if (args[arg].Length > 1)
                            password = DecodeUmlaute(args[arg]);
                        break;
                    case "CompanyId":
                        companyId = int.Parse(args[arg]);
                        break;
                    case "Email":
                        email = DecodeUmlaute(args[arg]);
                        break;
                    case "Telefon":
                        phone = GsmConverter.StrToPhone(args[arg]);
                        break;
                    case "Max_Inaktivität":
                        maxInactivity = int.Parse(args[arg]);
                        break;
                    case "SendSms":
                        if (args[arg].ToLower() == "on")
                            sendSms = 1;
                        else
                            sendSms = 0;
                        break;
                    case "SendEmail":
                        if (args[arg].ToLower() == "on")
                            sendEmail = 1;
                        else
                            sendEmail = 0;
                        break;
                    default:
                        break;
                }
            }

            if (createNewAccount)
            {
                if (password.Length < 4)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler - Passwort ungültig", "Das vergebene Passwort entspricht nicht den Vorgaben. Der Benutzer wird nicht erstellt."));
                }
                else
                {
                    int newId = Program.Sql.InsertContact(name, password, companyId, email, phone, maxInactivity, sendSms == 1, sendEmail == 1);
                    if (newId == 0)
                    {
                        builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Schreiben in die Datenbank", "Der Benutzer '" + name + "' konnte nicht erstellt werden."));
                    }
                    else
                    {
                        builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' erstellt", "Der Benutzer '" + name + "' wurde in der Datenbank neu erstellt."));
                    }
                }
            }
            else
            {
                if (!Program.Sql.UpdateContact(contactId, name, password, companyId, phone, sendSms, email, sendEmail, string.Empty, maxInactivity))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Änderungen für Benutzer '" + name + "'", "Die Änderungen konnten nicht in die Datenbank übertragen werden."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Änderungen für Benutzer '" + name + "' übernommen", "Die Änderungen an Benutzer '" + name + "' wurden in der Datenbank gespeichert."));
                }
            }

            return builder.ToString();
        }

        public static string ProcessFormShift(Dictionary<string, string> args, int logedInUserId, bool isAdmin)
        {
            StringBuilder builder = new StringBuilder();

            int shiftUserId = logedInUserId;
            int shiftId = 0;
            string name = "-KEIN NAME-";
            DateTime date = DateTime.MinValue;
            int beginHour = 17;
            int endHour = 7;
            bool createWeekShift = false;
          //  bool createNewShift = false;

            foreach (string arg in args.Keys)
            {
                switch (arg)
                {
                    case "selectedRow":
                        if (args[arg].StartsWith("Datum_"))
                        {
                            DateTime.TryParse(args["selectedRow"].ToString().Substring(6), out date);                            
                        }
                        else
                        {
                            int.TryParse(args["selectedRow"].ToString(), out shiftId);
                        }
                        break;
                    case "ContactId":
                        int.TryParse(args[arg].ToString(), out shiftUserId);                        
                        break;
                    case "Name":
                        name = DecodeUmlaute(args[arg].Replace('+', ' '));
                        break;
                    case "Datum":
                        if (date == DateTime.MinValue)
                            date = DateTime.Parse(args["Datum"].ToString()).Date;
                        break;
                    case "Beginn":
                        beginHour = int.Parse(args[arg]);
                        break;
                    case "Ende":
                        endHour = int.Parse(args[arg]);
                        break;
                    case "CreateWeekShift":
                        // if (args[arg].ToLower() == "on")
                        createWeekShift = true;
                        break;
                }
            }

            if (shiftUserId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Zuordnung", string.Format("Der Benutzer konnte der Bereitschaft nicht zugewiesen werden.")));
                return builder.ToString();
            }

            if (shiftId == 0)
            {
                //Neu erstellen
                DateTime firstStartTime = DateTime.Now;
                DateTime lastEndTime = DateTime.Now;

                if (createWeekShift)
                {
                    date = date.AddDays(DayOfWeek.Monday - date.DayOfWeek);

                    for (int i = 0; i < 7; i++)
                    {
                        DateTime StartTime = MelBoxSql.ShiftStandardStartTime(date);
                        DateTime EndTime = MelBoxSql.ShiftStandardEndTime(date);

                        if (i == 0) firstStartTime = StartTime;
                        if (i == 6) lastEndTime = EndTime;

                        Program.Sql.InsertShift(shiftUserId, StartTime, EndTime);
                        date = date.AddDays(1);
                    }
                }
                else
                {
                    DateTime StartTime = date.AddHours(beginHour);
                    DateTime EndTime = date.AddDays(1).AddHours(endHour);

                    firstStartTime = StartTime;
                    lastEndTime = EndTime;

                    Program.Sql.InsertShift(shiftUserId, StartTime, EndTime);
                }

                builder.Append(MelBoxWeb.HtmlAlert(3, "Neue Bereitschaft erstellt", string.Format("Neue Bereitschaft für '{0}' vom {1} bis {2} erstellt.", name, firstStartTime, lastEndTime)));
            }
            else
            {
                //Update
                if (shiftUserId != logedInUserId && !isAdmin)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Nicht änderbar", string.Format("Sie können nur ihre eigenen Bereitschaftszeiten bearbeiten.")));
                }

                ////Admin: Darstellung anderer User
                //if (shiftUserId == 0) contactId = logedInContactId;
                //else logedInContactId = contactId;

                DateTime StartTime = date.AddHours(beginHour);
                DateTime EndTime = date.AddDays(1).AddHours(endHour);

                if (!Program.Sql.UpdateShift(shiftId, shiftUserId, StartTime, EndTime))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Bereitschaft  Nr. " + shiftId + " bearbeiten",
                        string.Format("Die Bereitschaft konnte nicht in der Datenbank geändert werden.")));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Bereitschaft Nr. " + shiftId + " geändert",
                       string.Format("Die Bereitschaft Nr. {0} wurde geändert auf {1} im Zeitraum {2} bis {3}.", shiftId, name, StartTime, EndTime)));
                }

            }


            return builder.ToString();
        }

    }

    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsAdmin { get; set; } = false;

        //public int CompanyId { get; set; }

        // public string Email { get; set; }

        // public ulong Phone { get; set; }

    }


}
