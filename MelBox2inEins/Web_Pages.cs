using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    [RestResource]
    public class MelBoxResource
    {



#if DEBUG
        private int LogedInContactId = 0;
#else
        private int LogedInContactId = 0;
#endif
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/log")]
        public IHttpContext ShowMelBoxLog(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewLog(DateTime.UtcNow, DateTime.UtcNow);

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/in")]
        public IHttpContext ShowMelBoxIn(IHttpContext context)
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                DataTable dt = Program.Sql.GetViewMsgRec();
                builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
                builder.Append(MelBoxWeb.HtmlTablePlain(dt, true));
                builder.Append(MelBoxWeb.HtmlEditor("/blocked/add", "Ausgewählte Nachricht sperren"));
                builder.Append(MelBoxWeb.HtmlFoot());
            }

            catch
            {
                builder.Append(MelBoxWeb.HtmlHead("Fehler beim Laden"));
                builder.Append(MelBoxWeb.HtmlAlert(1, "SQL-Fehler", "Die angeforderte Abfrage 'GetViewMsgRec()' konnte nicht fehlerfrei ausgeführt werden."));
                builder.Append(MelBoxWeb.HtmlFoot());
            }

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/out")]
        public IHttpContext ShowMelBoxOut(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgSent();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(builder.ToString());
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/overdue")]
        public IHttpContext ShowMelBoxOverdue(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgOverdue();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        #region Gesperrte Nachrichten
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/blocked")]
        public IHttpContext ShowMelBoxBlocked(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgBlocked();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt, true));
            builder.Append(MelBoxWeb.HtmlEditor("/blocked/remove", "Aus Sperrliste entfernen"));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/add")]
        public IHttpContext AddMelBoxBlocked(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Nachricht sperren"));

            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
            }
            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int recMsgId))
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht an die Sperrliste übergeben."));
            }
            else
            {
                int contentId = Program.Sql.GetContentId(recMsgId);

                if (contentId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
                }
                else
                {
                    Program.Sql.InsertMessageBlocked(contentId);
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht in die Sperrliste aufgenommen", "Die Nachricht mit der Id " + contentId + " wird nicht mehr in die Bereitschaft weitergeleitet."));
                }
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/remove")]
        public IHttpContext RemoveMelBoxBlocked(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Nachricht entsperren"));

            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
            }
            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int contentId))
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht zum entsperren übergeben."));
            }
            else
            {
                if (contentId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
                }
                else
                {
                    Program.Sql.DeleteMessageBlocked(contentId);
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht aus der Sperrliste genommen", "Die Nachricht mit der Id " + contentId + " wird wieder in die Bereitschaft weitergeleitet."));
                }
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }
        #endregion

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/shift")]
        public IHttpContext ShowMelBoxShift(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewShift();

            StringBuilder builder = new StringBuilder();
            //     builder.Append(MelBoxWeb.HtmlHead(dt.TableName, LogedInContactId));
            builder.Append(MelBoxWeb.HtmlTableShift(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
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
            //  var request = context.Request.QueryString["contactid"] ?? "1";
            var request = "7"; //TEST

            StringBuilder builder = new StringBuilder();

            if (!int.TryParse(request, out int contactId))
            {
                builder.Append(MelBoxWeb.HtmlHead("Ungültige Eingabe"));
                builder.Append("<div class='w3-panel w3-yellow'><h3>Fehleingabe!</h3 >");
                builder.Append("<p>Die Seite wurde mit einem ung&uuml;ltigen Parameter aufgerufen.</p>");
                builder.Append(MelBoxWeb.HtmlFoot());
            }
            else
            {
                DataTable dt = Program.Sql.GetViewContactInfo(contactId);
                DataTable dtCompany = Program.Sql.GetAllCompanys();

                builder.Append(MelBoxWeb.HtmlHead(dt.TableName));

                builder.Append("</center><div class='w3-card'>\n");
                builder.Append("<form class='w3-container' id='form1' action='/account/safe'>\n");

                foreach (DataRow r in dt.Rows)
                {
                    foreach (DataColumn c in dt.Columns)
                    {
                        switch (c.ColumnName)
                        {
                            case "ContactId":
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>Laufende Nummer</b></label>\n");
                                builder.Append(" <input class='w3-input w3-border w3-light-grey' name='" + c.ColumnName + "' id='" + c.ColumnName + "' type='text' value='" + r[c.ColumnName] + "' disabled>\n</p>\n");
                                break;
                            case "Passwort":
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>" + c.ColumnName + "</b></label>\n");
                                builder.Append(" <input class='w3-input w3-border w3-sand' name='" + c.ColumnName + "' id='" + c.ColumnName + "' placeholder='Passwort' type='password' >\n</p>\n");
                                break;
                            case "CompanyId":
                                builder.Append("<div style='display:none;' name='" + c.ColumnName + "' id='" + c.ColumnName + "'>" + r[c.ColumnName] + "</div>");
                                break;
                            case "Firma":
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>" + c.ColumnName + "</b></label>\n");
                                builder.Append("  <select class='w3-select w3-border w3-sand' name='" + c.ColumnName + "' id='" + c.ColumnName + "'>\n");
                                // builder.Append("   <option value='" + r[c.ColumnName] + "' selected>" + r[c.ColumnName] + "</option>\n"); //TEST

                                foreach (DataRow row in dtCompany.Rows)
                                {
                                    string companyName = MelBoxWeb.EncodeUmlaute(row["Name"].ToString());

                                    builder.Append("   <option value='" + row["Id"] + "' ");
                                    builder.Append(r[c.ColumnName] == row["Id"] ? "selected" : string.Empty);
                                    builder.Append(">" + companyName + "</option>\n");
                                }

                                builder.Append("  </select>\n");
                                break;
                            case "SendSms":
                            case "SendEmail":
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>" + c.ColumnName + "</b></label>\n");
                                builder.Append(string.Format("<input class='w3-check w3-block' type='checkbox' name='{0}' id='{0}' {1}></p>", c.ColumnName, (int.Parse(r[c.ColumnName].ToString()) > 0) ? "checked" : string.Empty));
                                break;
                            case "Max_Inaktivität":
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>Maximale Inaktiviät (Stunden)</b></label>\n");
                                builder.Append(" <input class='w3-input w3-border w3-sand' name='" + c.ColumnName + "' id='" + c.ColumnName + "' type='number' min='0' step='8' value ='" + r[c.ColumnName] + "'>\n</p>\n");
                                break;
                            default:
                                builder.Append("<p class='w3-half w3-margin'>\n <label class='w3-text-grey'><b>" + c.ColumnName + "</b></label>\n");
                                builder.Append(" <input class='w3-input w3-border w3-sand' name='" + c.ColumnName + "' id='" + c.ColumnName + "' placeholder='" + c.ColumnName + "' type='text' value='" + r[c.ColumnName] + "'>\n</p>\n");
                                break;
                        }

                    }
                }
                builder.Append("<p class='w3-half w3-margin'>\n");
                builder.Append("  <input type='button' class='w3-button w3-teal' onclick =\"document.getElementById('modal1').style.display = 'block'\" value='Speichern'></input> </p>");

                builder.Append("</form></div>\n");


                builder.Append("<div id='modal1' class='w3-modal'>\n");
                builder.Append(" <div class='w3-modal-content w3-card-4'>\n");
                builder.Append("  <header class='w3-container w3-teal'>\n");
                builder.Append("  <span onclick=\"document.getElementById('modal1').style.display = 'none'\" class='w3-button w3-display-topright'>&times;</span>\n");
                builder.Append("  <h2>Wirklich speichern?</h2>\n</header>\n");
                builder.Append("  <div class='w3-container'><p>");
                builder.Append("  <script> </script>");
                builder.Append("  <input class='w3-input w3-border w3-teal' type='submit' formmethod='post' form='form1' value='Speichern'>\n</p>\n");
                builder.Append("  </p></div>");
                builder.Append("</div></div></div>");

                builder.Append("<center>\n");

                builder.Append(MelBoxWeb.HtmlFoot());
            }

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        /// <summary>
        /// BAUSTELLE
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/safe")]
        public IHttpContext AccountSafe(IHttpContext context)
        {
            // ContactId, Name, Passwort, CompanyId, Firma, Email, Telefon, SendSms , SendEmail, Max_Inaktivität 

            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
            string[] args = payload.Split('&');

            int contactId = 0;
            string name = "-KEIN NAME-";
            string password = null;
            int companyId = 1;
            //string firma;
            string email = null;
            ulong phone = 0;
            string sendSms;
            string sendEmail;
            string maxInactivity;


            foreach (string arg in args)
            {
                if (arg.StartsWith("ContactId="))
                {
                    contactId = int.Parse(arg.Split('=')[1]);
                }

                if (arg.StartsWith("Name="))
                {
                    name = arg.Split('=')[1].Replace('+', ' ');
                }

                if (arg.StartsWith("Passwort="))
                {
                    password = arg.Split('=')[1];
                }

                if (arg.StartsWith("CompanyId="))
                {
                    companyId = int.Parse(arg.Split('=')[1]);
                }

                if (arg.StartsWith("Email="))
                {
                    email = arg.Split('=')[1];
                }

                if (arg.StartsWith("Telefon="))
                {
                    phone = GsmConverter.StrToPhone(arg.Split('=')[1]);
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("&Auml;nderung Benutzer"));

            if (0 == Program.Sql.UpdateContact(contactId, name, password, companyId, email, phone))
            {
                builder.Append("<div class='w3-panel w3-yellow w3-border'><h3>Keine &Auml;nderungen für Benutzer '" + name + "'</h3></div>");
                builder.Append("<p>Es wurden keine &Auml;nderungen &uuml;bergeben oder der Aufruf war fehlerhaft.</p>");
            }
            else
            {
                builder.Append("<div class='w3-panel w3-pale-green w3-border'><h3>&Auml;nderungen für Benutzer '" + name + "' gespeichert</h3></div>");
                builder.Append("<p>Die &Auml;nderungen an Benutzer '" + name + "' wurden in der Datenbank gespeichert.</p>");
            }

            builder.Append("<p>Id\t" + contactId);
            builder.Append("<br>Name\t" + name);
            builder.Append("<br>Email\t" + email);
            builder.Append("<br<Telefon\t" + phone + "</p>");

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
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

            LogedInContactId = 0;

            StringBuilder builder = new StringBuilder();

#if DEBUG
            LogedInContactId = 1;
#else

            if (name.Length > 3 && password.Length > 3)
            {
                if (password == MelBoxWebServer.MasterPassword)
                {
                    LogedInContactId = 1;
                }
                else
                {
                    LogedInContactId = Program.Sql.GetContactIdFromLogin(name, password);
                }
            }
#endif
            //   builder.Append(MelBoxWeb.HtmlHead("Log-In", LogedInContactId));
            builder.Append("<div class='w3-panel " + (LogedInContactId != 0 ? "w3-pale-green" : "w3-yellow") + " w3-border'>\n");
            builder.Append(" <h2>LogIn " + (LogedInContactId != 0 ? "erfolgreich" : "fehlgeschlagen") + "</h2>");
            builder.Append(" <p></p>");
            //builder.Append("<script>\n"); 
            //builder.Append("  if (typeof(Storage) !== \"undefined\") {");
            //builder.Append("    localStorage.LogIn = \"" + LogedInContactId + "\"");

            ////document.getElementById("result").innerHTML = localStorage.lastname
            //builder.Append("</script>\n");
            builder.Append("</div>\n");
            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(builder.ToString());
            return context;

            //http://localhost:1234/repeat?word=parrot
            //var password = context.GetPropertyValueAs<string>("password");

            //var password = context.Request.QueryString["password"] ?? "what?";
        }


        [RestRoute]
        public IHttpContext Home(IHttpContext context)
        {
            #region Payload
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            string guid = string.Empty;
            string alert = string.Empty;


            if (args.ContainsKey("name") && args.ContainsKey("password"))
            {
                string name = args["name"].Replace('+', ' ');
                string password = args["password"];

                int myId = Program.Sql.GetContactIdFromLogin(name, password);

                if (myId != 0) //Es wurde ein Benutzer mit diesen Zugangsdaten gefunden
                {
                    Program.Sql.Log(MelBoxSql.LogTopic.Sql, MelBoxSql.LogPrio.Info, "Benutzer " + myId + "(" + name + ") angemeledt.");

                    guid = MelBoxWeb.GenerateID(myId);
                    alert = MelBoxWeb.HtmlAlert(3, "LogIn erfolgreich", string.Format("Sie haben sich erfolgreich als Benutzer {0} '" + name + "' mit der ID {1} eingeloggt.", myId, guid));
                }
                else
                {
                    alert = MelBoxWeb.HtmlAlert(2, "LogIn fehlgeschlagen", string.Format("Der Benutzername oder das Passwort sind unbekannt.", myId, guid));
                }
            }

            #endregion

            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlHead("St&ouml;rmeldesystem f&uuml;r Kreutztr&auml;ger K&auml;ltetechnik", guid));
            builder.Append(alert);
            builder.Append(MelBoxWeb.HtmlLogIn());

            #region TEST
#if DEBUG
            if (MelBoxWeb.LogedInGuids.Count > 0)
            {
                builder.Append("<p>LogedIn</p> <ul class='w3-ul'>\n");

                foreach (var item in MelBoxWeb.LogedInGuids.Keys)
                {
                    builder.Append("<li>" + MelBoxWeb.LogedInGuids[item] + "\t" + item + "</li>\n");
                }
                builder.Append("</ul>\n");
            }
#endif
            #endregion

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }
    }

}
