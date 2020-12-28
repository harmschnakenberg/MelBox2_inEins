using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2
{
    public class MelBoxWebServer
    {
        private static bool RunWebServer = true; //Schalter zum Ausschalten des Webservers

        private static string Port { get; set; }

        public static void StartWebServer(string port = "48040")
        {
            Port = port;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var _ = new MelBoxWebServer();                
            }).Start();
        }

        public static void StopWebServer()
        {
            RunWebServer = false;
        }

        public MelBoxWebServer()
        {
            RunWebServer = true;

            using (var server = new RestServer())
            {
                server.Port = Port;                
                server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Warn).Start();
                Console.WriteLine("WebHost:\thttp://" + server.Host + ":" + server.Port);

                while (RunWebServer)
                {
                    //nichts unternehmen 
                }
                server.LogToConsole().Stop();
                //server.Stop();
                server.ThreadSafeStop();
            }
        }

        public static string ReplaceUmlaute(string input)
        {
            return input.Replace("Ä", "&Auml;").Replace("Ö", "&Ouml;").Replace("Ü", "&Uuml;").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;");
        }

        #region Bausteine
        public static string HtmlHead(string htmlTitle, bool logedIn = false)
        {
            string disabled = logedIn ? string.Empty : "disabled";

            StringBuilder builder = new StringBuilder();
            builder.Append("<html>\n");
            builder.Append("<head>\n");
            builder.Append("<link rel='shortcut icon' href='https://www.kreutztraeger-kaeltetechnik.de/wp-content/uploads/2016/12/favicon.ico'>\n");
            builder.Append("<title>");
            builder.Append("MelBox2 - " + htmlTitle);
            builder.Append("</title>\n");
            builder.Append("<meta name='viewport' content='width=device-width, initial-scale=1'>\n");
            builder.Append("<link rel='stylesheet' href='https://www.w3schools.com/w3css/4/w3.css'>\n");
            builder.Append("<link rel='stylesheet' href='https://fonts.googleapis.com/icon?family=Material+Icons+Outlined'>\n");
            builder.Append("</head>\n");
            builder.Append("<body>\n");
            builder.Append("<p><span class='w3-display-topright'>" + DateTime.Now + "</span></p>\n");
            builder.Append("<div class='w3-bar w3-border'>\n");
            //builder.Append("<a href='.\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>menu</i></a>\n");
            //builder.Append("<a href='.\\in' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>drafts</i></a>\n");
            //builder.Append("<a href='.\\out' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>redo</i></a>\n");
            //builder.Append("<a href='.\\overdue' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>alarm</i></a>\n");
            //builder.Append("<a href='.\\blocked' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></a>\n");
            //builder.Append("<a href='.\\shift' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined' style='color:lightgrey'>event</i></a>\n");
            //builder.Append("<a href='.\\account' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined' style='color:lightgrey'>contact_page</i></a>\n");
            //builder.Append("<a href='.\\login' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>login</i></a>\n");

            builder.Append("<a href='.\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>login</i></a>\n");
            builder.Append("<button onclick=\"document.location='\\in'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>drafts</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\out'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>redo</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\overdue'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>alarm</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\blocked'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\shift'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>event</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\account'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\log'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>assignment</i></button>\n");
            builder.Append("<button onclick=\"document.location='\\'\" class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>person</i></button>\n");


            builder.Append("</div>\n");
            builder.Append("<center>\n");
            builder.Append("<div class='w3-container w3-cyan'>\n");
            builder.Append("<h1>MelBox2 - " + htmlTitle + "</h1>\n</div>\n\n");

            return builder.ToString();
        }
        public static string HtmlFoot()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("</center>\n");
            builder.Append("<p>&nbsp;</p>");
            builder.Append("<div class='w3-container w3-display-bottom w3-cyan'>");
            builder.Append("<p>&nbsp;</p>");
            builder.Append("</div>\n</body>\n</html>");
            return builder.ToString();
        }
        #endregion

        #region Tabellen
        public static string HtmlTablePlain(DataTable dt)
        {
            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            StringBuilder builder = new StringBuilder();
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell'>\n");
            builder.Append("<tr class='w3-teak'>\n\t");
            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");
            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr>\n\t");
                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");
                    builder.Append(r[c.ColumnName]);
                    builder.Append("</td>");
                }
                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>\n");

            return builder.ToString();
        }

        //BAUSTELEL: Fehlt
        //private static string ToHTML_ShiftTable(DataTable dt)
        //{

        //}

        //BAUSTELLE : Bitzuordnung zu Wochentagen passt noch nicht!
        public static string HTMLTableBlocked(DataTable dt, bool logedIn = false)
        {
            string disabled = logedIn ? string.Empty : "disabled";

            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlHead(dt.TableName));
            builder.Append("</div><div class='w3-row'>\n");
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell'>\n");
            builder.Append("<tr class='w3-teak'>\n");
            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");
            foreach (DataRow r in dt.Rows)
            {
                int index = dt.Rows.IndexOf(r);

                builder.Append("<tr>\n");
                foreach (DataColumn c in dt.Columns)
                {
                    string update = string.Empty;
                    builder.Append("<td>");

                    switch (c.ColumnName)
                    {
                        case "Beginn":
                        case "Ende":
                            builder.Append("<select name='" + c.ColumnName + "' " + disabled + ">\n");
                            string selected = string.Empty;

                            for (int i = 0; i < 24; i++)
                            {
                                if (i + " Uhr" == r[c.ColumnName].ToString())
                                {
                                    selected = "selected";
                                }
                                else
                                {
                                    selected = string.Empty;
                                }

                                builder.Append(" <option value='" + i + "' " + selected + ">" + i + " Uhr</option>\n");
                            }
                            builder.Append("</select>");
                            break;
                        case "So":
                        case "Mo":
                        case "Di":
                        case "Mi":
                        case "Do":
                        case "Fr":
                        case "Sa":
                            string check = string.Empty;
                            if (r[c.ColumnName].ToString() == "1")
                                check = "checked='checked' ";
                            builder.Append("<input form='" + index + "' name='" + c.ColumnName + "' class='w3-check' type='checkbox' " + check + " " + disabled + ">");
                            break;
                        default:
                            // builder.Append(r[c.ColumnName]);
                            builder.Append("<span form='" + index + "' name='" + c.ColumnName + "' >" + r[c.ColumnName].ToString() + "</span>");
                            break;
                    }

                    builder.Append("</td>\n");
                }
                builder.Append("<td><form action ='.\\blocked' method='post' id='" + index + "'><input type='submit' value='Speichern' " + disabled + "></form></td>");

                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>");
            builder.Append("</center>");
            builder.Append("</body>");
            builder.Append("</html>");

            return builder.ToString();
        }

        #endregion
    }

    [RestResource]
    public class MelBoxResource
    {
        private bool logedIn = false;

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/log")]
        public IHttpContext ShowMelBoxLog(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewLog(DateTime.UtcNow, DateTime.UtcNow);

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWebServer.HtmlHead(dt.TableName, logedIn) );
            builder.Append(MelBoxWebServer.HtmlTablePlain(dt));
            builder.Append(MelBoxWebServer.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/in")]
        public IHttpContext ShowMelBoxIn(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgRec();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWebServer.HtmlHead(dt.TableName, logedIn));
            builder.Append(MelBoxWebServer.HtmlTablePlain(dt));
            builder.Append(MelBoxWebServer.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/out")]
        public IHttpContext ShowMelBoxOut(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgSent();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWebServer.HtmlHead(dt.TableName, logedIn));
            builder.Append(MelBoxWebServer.HtmlTablePlain(dt));
            builder.Append(MelBoxWebServer.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/overdue")]
        public IHttpContext ShowMelBoxOverdue(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgOverdue();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWebServer.HtmlHead(dt.TableName, logedIn));
            builder.Append(MelBoxWebServer.HtmlTablePlain(dt));
            builder.Append(MelBoxWebServer.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/blocked")]
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked")]
        public IHttpContext ShowMelBoxBlocked(IHttpContext context)
        {
            //Bei Update Datenbank aktualisieren
            var id = context.Request.QueryString["id"] ?? "what?";
            var date = context.Request.QueryString["id"] ?? "what?";
            var begin = context.Request.QueryString["id"] ?? "what?";
            var end = context.Request.QueryString["id"] ?? "what?";

            //Tabelle der letzten ausgegangenen Nachrichten abrufen, formatieren und wegsenden
            var word = MelBoxWebServer.HTMLTableBlocked(Program.Sql.GetViewMsgBlocked());
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/repeat")]
        public IHttpContext RepeatMe(IHttpContext context)
        {
            //http://localhost:1234/repeat?word=parrot
            var word = context.Request.QueryString["word"] ?? "what?";
            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/account")]
        public IHttpContext ShowMelBoxAccount(IHttpContext context)
        {
            var request = context.Request.QueryString["contactid"] ?? "1";
            var word = "<html><</html>";

            if (int.TryParse(request, out int contactId))
            {
                word = MelBoxWebServer.HtmlTablePlain(Program.Sql.GetViewContactInfo(contactId));
            }
            else
            {
                word = MelBoxWebServer.HtmlHead("Ungültige Eingabe") + "<p>Der angefragte Kontakt konnte nicht gefunden werden.<br>Botte EIngabe prüfen.</p></center></body></html>";
            }

            context.Response.SendResponse(word);
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/login")]
        public IHttpContext LogIn(IHttpContext context)
        {
            string name = string.Empty;
            string password = string.Empty;
            string payload = context.Request.Payload;
            string[] args = payload.Split('&');

            foreach (string arg in args)
            {
                if (arg.StartsWith("name="))
                {
                    name = arg.Split('=')[1];
                }

                if (arg.StartsWith("password="))
                {
                    password = arg.Split('=')[1];
                }
            }

            if (name.Length < 3 || password.Length < 3) return context;

            if (password.StartsWith("password="))
            {
                password = password.Split('=')[1];
            }

            if (password == "1234")
            {
                logedIn = true;
            }
            else
            {
                //TODO Benutzer Passswort prüfen
            }

            //http://localhost:1234/repeat?word=parrot
            //var password = context.GetPropertyValueAs<string>("password");

            //var password = context.Request.QueryString["password"] ?? "what?";


            return context;
        }

        [RestRoute]
        public IHttpContext Home(IHttpContext context)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWebServer.HtmlHead("St&ouml;rmeldesystem f&uuml;r Kreutztr&auml;ger K&auml;ltetechnik", logedIn));

            builder.Append("<footer class='w3-card w3-half w3-display-middle'>\n"); //w3-quarter 
            builder.Append("  <div class='w3-container w3-cyan'>\n");
            builder.Append("    <h2>LogIn</h2>\n");
            builder.Append("  </div>\n");
            builder.Append("  <form class='w3-container' action='/login' method='post'>\n");
            builder.Append("    <label class='w3-text-grey'><b>Benutzer</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='name' type='text' required></p>\n");
            builder.Append("    <label class='w3-text-grey'><b>Passwort</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='password' type='password' required></p>\n");
            builder.Append("    <p>\n");
            builder.Append("    <button class='w3-button w3-teal'>LogIn</button></p>\n");
            builder.Append("  </form>\n");
            builder.Append("</footer>\n");

            builder.Append(MelBoxWebServer.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }
    }


}
