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

        public static string CheckLogIn(string name, string password)
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

#if DEBUG
            Console.WriteLine("Angemeldet:");
            foreach (string item in LogedInUsers.Keys)
            {
                User x = LogedInUsers[item];

                Console.WriteLine("{0}\t{1}\t{2}", x.IsAdmin ? "Admin" : "Benutzer", x.Id, x.Name);
            }
#endif
            return guid;
        }

        public static User GetUserFromGuid(string guid)
        {
            // User user = new User();
            if (LogedInUsers.ContainsKey(guid))
                return LogedInUsers[guid];
            else
                return null;
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
