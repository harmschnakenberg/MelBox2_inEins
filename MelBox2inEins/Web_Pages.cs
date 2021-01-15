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
    public class TestResource
    {
        #region internes

        #region Felder
        string guid = string.Empty;
        string requestingPage = string.Empty;
        string logedInUserName = string.Empty;
        int logedInUserId = 0;
        bool isAdmin = false;
        #endregion

        private void ReadGlobalFields(Dictionary<string, string> args)
        {
            if (args.ContainsKey("guid"))
            {
                guid = args["guid"];
                User user = MelBoxWeb.GetUserFromGuid(guid);

                if (user != null)
                {
                    logedInUserName = user.Name;
                    logedInUserId = user.Id;
                    isAdmin = user.IsAdmin;
                }
            }

            if (args.ContainsKey("pageTitle"))
            {
                requestingPage = args["pageTitle"];
            }
#if DEBUG
            Console.WriteLine("Aufruf von {0} durch: [{1}] {2} - Admin: {3} | {4}", requestingPage, logedInUserId, logedInUserName, isAdmin, guid);
#endif
        }

        #endregion

        //[RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/repeat")]
        //public IHttpContext RepeatMe(IHttpContext context)
        //{
        //    var word = context.Request.QueryString["word"] ?? "what?";
        //    context.Response.SendResponse(word);
        //    return context;
        //}

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/in")]
        public IHttpContext ResponseIn(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            Dictionary<string, string> action = new Dictionary<string, string>();

            if (isAdmin)
            {
                action.Add("/blocked/create", "Ausgewählte Nachricht sperren");
            }

            DataTable dt = Program.Sql.GetViewMsgRec();
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTableIn(dt, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/out")]
        public IHttpContext ResponseOut(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            DataTable dt = Program.Sql.GetViewMsgSent();
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/overdue")]
        public IHttpContext ResponseOverdue(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            DataTable dt = Program.Sql.GetViewMsgOverdue();
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }



        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/log")]
        public IHttpContext ResponseLog(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            DataTable dt = Program.Sql.GetViewLog(DateTime.UtcNow, DateTime.UtcNow);

            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked")]
        public IHttpContext ResponseBlocked(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            Dictionary<string, string> action = new Dictionary<string, string>();

            if (isAdmin)
            {
                action.Add("/blocked/update", "Gesperrte Nachricht bearbeiten");
                action.Add("/blocked/delete", "Aus Sperrliste entfernen");
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTableBlocked(dt, 0, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }



        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift")]
        public IHttpContext ResponseShift(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            Dictionary<string, string> action = new Dictionary<string, string>
            {
              //  { "/shift/create", "Bereitschaft neu anlegen" },
                { "/shift/edit", "Bereitschaft bearbeiten" }
            };

            if (isAdmin)
            {
                action.Add("/shift/delete", "Bereitschaft löschen");
            }

            DataTable dt = Program.Sql.GetViewShift();
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlTableShift(dt, 0, logedInUserId, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/edit")]
        public IHttpContext ResponseShiftEdit(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);
            int shiftId = 0;
            DateTime date = DateTime.MinValue;

            ReadGlobalFields(args);

            DataTable dt = Program.Sql.GetViewShift();
            StringBuilder builder = new StringBuilder();

            if (args.ContainsKey("selectedRow"))
            {
                if (args["selectedRow"].StartsWith("Datum_"))
                {
                    DateTime.TryParse(args["selectedRow"].ToString().Substring(6), out date);
                }
                else
                {
                    int.TryParse(args["selectedRow"].ToString(), out shiftId);
                }
            }

            builder.Append(MelBoxWeb.HtmlFormShift(date, shiftId, logedInUserId, isAdmin));

#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/update")]
        public IHttpContext ResponseShiftCreateOrUpdate(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (logedInUserId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormShift(args, logedInUserId, isAdmin));
            }
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Bereitschaft erstellen", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/delete")]
        public IHttpContext ResponseShiftDelete(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();
            int shiftId = 0;
            if (args.ContainsKey("selectedRow"))
            {
                int.TryParse(args["selectedRow"].ToString(), out shiftId);
            }

            if (logedInUserId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
            }
            else if(!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie besitzen nicht die notwendigen Rechte für diese Aktion."));
            }
            else if (shiftId == 0 )
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Bereitschaft löschen", "Fehlerhafter Aufruf. Die zu löschende Bereitschaft konnte nicht zugeordnet werden."));
            }
            else
            {
                if (!Program.Sql.DeleteShift(shiftId))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Bereitschaft  Nr. " + shiftId + " löschen",
                        string.Format("Die Bereitschaft konnte nicht aus der Datenbank gelöscht werden.")));
                }
                else
                {
                    string msg = string.Format("Die Bereitschaft Nr. {0} wurde durch den Benutzer '{1}' gelöscht.", shiftId, logedInUserName);

                    builder.Append(MelBoxWeb.HtmlAlert(3, "Bereitschaft Nr. " + shiftId + " gelöscht", msg));

                    Program.Sql.Log(MelBoxSql.LogTopic.Shift, MelBoxSql.LogPrio.Warning, msg);
                }
            }
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Bereitschaft löschen", logedInUserName));
            return context;
        }

        #region Kontakt

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account")]
        public IHttpContext ResponseAccount(IHttpContext context)
        {
            string caption = "Benutzerkonto";
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            Dictionary<string, string> action = new Dictionary<string, string>
            {
                { "/account/update", "Änderungen an Kontakt speichern" }
            };

            if (isAdmin)
            {
                action.Add("/account/create", "Neuen Kontakt mit diesen Angaben einrichten");
                action.Add("/account/delete", "Diesen Kontakt löschen");
            }

            StringBuilder builder = new StringBuilder();

            int chosenContactId = logedInUserId;
            if (requestingPage == caption && args.ContainsKey("selectedContactId")) //Ist Antwort von dieser Seite
            {
                int.TryParse(args["selectedContactId"].ToString(), out chosenContactId);
            }
            else if (chosenContactId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Ungültiger Aufruf", "Für Einsicht Benutzerkonto bitte einloggen."));
            }

            if (logedInUserId != 0)
            {
                DataTable dt;
                if (isAdmin) dt = Program.Sql.GetViewContactInfo();
                else dt = Program.Sql.GetViewContactInfo(chosenContactId);

                DataTable dtCompany = Program.Sql.GetCompany();

                builder.Append(MelBoxWeb.HtmlFormAccount(dt, dtCompany, chosenContactId, isAdmin));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), caption, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/create")]
        public IHttpContext ResponseAccountCreate(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Benutzer anzulegen."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormAccount(args, true));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Benutzerkonto ändern", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/update")]
        public IHttpContext ResponseAccountUpdate(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin && int.Parse(args["ContactId"]) != logedInUserId)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Benutzer zu ändern."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormAccount(args, false));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Benutzerkonto anlegen", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/delete")]
        public IHttpContext ResponseAccountDelete(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Benutzer zu löschen."));
            }
            else
            {
                int.TryParse(args["ContactId"].ToString(), out int contactId);
                string name = args["Name"];

                if (contactId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler", "Der Benutzer '" + name + "' konnte nicht gelöscht werden. Der Aufruf war fehlerhaft."));
                }
                else
                {
                    if (!Program.Sql.DeleteContact(contactId))
                    {
                        builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler beim löschen von '" + name + "'", "Der Benutzer " + contactId + " '" + name + "' konnte nicht aus der Datenbank gelöscht werden."));
                    }
                    else
                    {
                        builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' gelöscht", "Der Benutzer " + contactId + " '" + name + "' wurde aus der Datenbank gelöscht."));
                    }
                }
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Benutzerkonto löschen", logedInUserName));
            return context;
        }

        #endregion

        [RestRoute]
        public IHttpContext Login(IHttpContext context)
        {
            string caption = "Login"; //Überschriftd er Seite
            string newGuid = string.Empty; //Nur füllen, wenn neue Benutzeranmeldung
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);
            StringBuilder builder = new StringBuilder();

            if (args.ContainsKey("name") && args.ContainsKey("password"))
            {
                string name = MelBoxWeb.DecodeUmlaute(args["name"].Replace('+', ' '));
                string password = MelBoxWeb.DecodeUmlaute(args["password"]);

                newGuid = MelBoxWeb.CheckLogin(name, password);
                guid = newGuid;
                logedInUserName = name;

                if (newGuid.Length == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Login fehlgeschlagen", "Login für Benutzer '" + name + "' fehlgeschlagen.<br>Benutzer und Passwort korrekt?"));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Willkommen " + name, "Login erfolgreich."));
                }
            }

            ReadGlobalFields(args);

            if (isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, "Angemeldet als Administrator ", string.Empty));
            }
            builder.Append(MelBoxWeb.HtmlLogin());
#if DEBUG           
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), caption, logedInUserName, newGuid));
            return context;
        }
    }
}
    #region 1
/*//    [RestResource]
//    public class MelBoxResource
//    {
//        #region nicht änderbare Tabellen
//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/log")]
//        public static IHttpContext ShowMelBoxLog(IHttpContext context)
//        {
//            DataTable dt = Program.Sql.GetViewLog(DateTime.UtcNow, DateTime.UtcNow);

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/in")]
//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/in/\w+$")]
//        public IHttpContext ShowMelBoxIn(IHttpContext context)
//        {
//            int logedInUserId = MelBoxWeb.LogedInAccountId(context.Request.RawUrl);

//            StringBuilder builder = new StringBuilder();
//            Dictionary<string, string> action = new Dictionary<string, string>
//            {
//                { "/blocked/create", "Ausgewählte Nachricht sperren" }
//            };

//            try
//            {
//                DataTable dt = Program.Sql.GetViewMsgRec();
//                builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//                builder.Append(MelBoxWeb.HtmlTablePlain(dt, logedInUserId));
//                builder.Append(MelBoxWeb.HtmlEditor(action));
//                builder.Append(MelBoxWeb.HtmlFoot());
//            }

//            catch
//            {
//                builder.Append(MelBoxWeb.HtmlHead("Fehler beim Laden"));
//                builder.Append(MelBoxWeb.HtmlAlert(1, "SQL-Fehler", "Die angeforderte Abfrage 'GetViewMsgRec()' konnte nicht fehlerfrei ausgeführt werden."));
//                builder.Append(MelBoxWeb.HtmlFoot());
//            }

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/out")]
//        public IHttpContext ShowMelBoxOut(IHttpContext context)
//        {
//            DataTable dt = Program.Sql.GetViewMsgSent();

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(builder.ToString());
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/overdue")]
//        public IHttpContext ShowMelBoxOverdue(IHttpContext context)
//        {
//            DataTable dt = Program.Sql.GetViewMsgOverdue();

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//            builder.Append(MelBoxWeb.HtmlTablePlain(dt));
//            builder.Append(MelBoxWeb.HtmlAccordeonInfo("Überfällige Meldungen", "Für überwachte Meldelinien werden überfällige Meldungen hier angezeigt. <br>Keine Einträge bedeutet: alles in Ordnung."));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }
*/
#endregion

#region Gesperrte Nachrichten
//*        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/blocked")]
//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/blocked/\w+$")]
//        public IHttpContext ShowMelBoxBlocked(IHttpContext context)
//        {
//            int logedInUserId = MelBoxWeb.LogedInAccountId(context.Request.RawUrl);

//            DataTable dt = Program.Sql.GetViewMsgBlocked();

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt, 0, logedInUserId) );
//            builder.Append(MelBoxWeb.HtmlAccordeonInfo("Gesperrte Meldungen", "Gesperrte Meldungen werden zu den eingestellten Zeiten nicht an die Bereitschaft weitergeleitet."));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/create")]
//        public IHttpContext AddMelBoxBlocked(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Nachricht sperren"));

//            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
//            }
//            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int recMsgId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht an die Sperrliste übergeben."));
//            }
//            else 
//            {
//                int logedInContactId = MelBoxWeb.LogedInGuids[args["guid"]];
//                bool isAdmin = MelBoxSql.AdminIds.Contains(logedInContactId);

//                if (!isAdmin)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie besitzen nicht die erforderliche Berechtigung für diese Aktion."));
//                }
//                else
//                {
//                    int contentId = Program.Sql.GetContentId(recMsgId);

//                    if (contentId == 0)
//                    {
//                        builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
//                    }
//                    else
//                    {
//                        Program.Sql.InsertMessageBlocked(contentId);
//                        string msg = "Die Nachricht Nr. " + contentId + " wurde durch den Benutzer " + logedInContactId + " gesperrt. Sie wird zu den eingestellten Zeiten nicht mehr in die Bereitschaft weitergeleitet.";

//                        builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht in die Sperrliste aufgenommen", msg));

//                        Program.Sql.Log(MelBoxSql.LogTopic.Shift, MelBoxSql.LogPrio.Warning, msg);
//                    }
//                }
//            }

//            DataTable dt = Program.Sql.GetViewMsgBlocked();

//            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt));
//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/update")]
//        public IHttpContext UpdateMelBoxBlocked(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);
//#if DEBUG
//            Console.WriteLine("/blocked/update Payload:");
//            foreach (var key in args.Keys)
//            {
//                Console.WriteLine(key + ":\t" + args[key]);
//            }
//#endif
//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Sperrzeiten ändern"));

//            int logedInUserId = 0;

//            if (MelBoxWeb.LogedInGuids.ContainsKey(args["guid"].ToString()))
//            {
//                logedInUserId = MelBoxWeb.LogedInGuids[args["guid"].ToString()];
//            }

//            if (logedInUserId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
//            }
//            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int contentId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht zum ändern übergeben."));
//            }
//            else
//            {
//                if (contentId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
//                }
//                else
//                {
//                    if (args.ContainsKey("Beginn") && args.ContainsKey("Ende"))
//                    {
//                        int beginHour = int.Parse(args["Beginn"].ToString());
//                        int endHour = int.Parse(args["Ende"].ToString());

//                        MelBoxSql.BlockedDays days = MelBoxSql.BlockedDays.None;
//                        if (args.ContainsKey("Mo")) days |= MelBoxSql.BlockedDays.Monday;
//                        if (args.ContainsKey("Di")) days |= MelBoxSql.BlockedDays.Tuesday;
//                        if (args.ContainsKey("Mi")) days |= MelBoxSql.BlockedDays.Wendsday;
//                        if (args.ContainsKey("Do")) days |= MelBoxSql.BlockedDays.Thursday;
//                        if (args.ContainsKey("Fr")) days |= MelBoxSql.BlockedDays.Friday;
//                        if (args.ContainsKey("Sa")) days |= MelBoxSql.BlockedDays.Saturday;
//                        if (args.ContainsKey("So")) days |= MelBoxSql.BlockedDays.Sunday;

//                        if (Program.Sql.UpdateMessageBlocked(contentId, beginHour, endHour, days))
//                        {
//                            builder.Append(MelBoxWeb.HtmlAlert(3, "Sperrzeiten geändert", "Die Sperrzeiten für Nachricht Nr. " + contentId + " wurden geändert."));
//                        }
//                        else
//                        {
//                            builder.Append(MelBoxWeb.HtmlAlert(2, "Sperrzeiten nicht geändert", "Die Sperrzeiten für Nachricht Nr. " + contentId + " konnten nicht geändert werden."));
//                        }
//                    }
//                }
//                //nur ausgwählte Nachricht anzeigen
//                DataTable dt = Program.Sql.GetViewMsgBlocked();
//                builder.Append(MelBoxWeb.HtmlUnitBlocked(dt, contentId, logedInUserId));

//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/delete")]
//        public IHttpContext RemoveMelBoxBlocked(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Nachricht entsperren"));

//            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
//            }
//            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int contentId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht zum entsperren übergeben."));
//            }
//            else
//            {
//                if (contentId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
//                }
//                else
//                {
//                    Program.Sql.DeleteMessageBlocked(contentId);
//                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht aus der Sperrliste genommen", "Die Nachricht mit der Nr. " + contentId + " wird wieder in die Bereitschaft weitergeleitet."));
//                }
//            }

//            DataTable dt = Program.Sql.GetViewMsgBlocked();
//            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        #endregion

//        #region Bereitschaft
//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/shift")]
//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/shift/\w+$")] //@"^/user/\d+$"
//       // [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/shift/\d+/\w+$")] 
//        public IHttpContext ShowMelBoxShift(IHttpContext context)
//        {
//            int logedInUserId = MelBoxWeb.LogedInAccountId(context.Request.RawUrl);

//            DataTable dt = Program.Sql.GetViewShift();

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
//            builder.Append(MelBoxWeb.HtmlUnitShift(dt, logedInUserId));
//            builder.Append(MelBoxWeb.HtmlAccordeonInfo("Bereitschaft", "Eine Bereitschafts-Einheit geht immer über einen Tageswechsel. Ab Uhrzeit 'Beginn' bis zum Folgetag Uhrzeit 'Ende' werden eingehende Nachrichten an den eingeteilten Kontakt gesendet."));
//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        //[RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/shift")]
//        //[RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/shift/\w+$")] //@"^/user/\d+$"
//        //public IHttpContext ShowMelBoxShiftLogedIn(IHttpContext context)
//        //{
//        //    StringBuilder builder = new StringBuilder();
//        //    DataTable dt = Program.Sql.GetViewShift();

//        //    builder.Append(MelBoxWeb.HtmlHead(dt.TableName));

//        //    int logedInUserId = 0;
//        //    string url = context.Request.RawUrl;
//        //    if (url.Length > 7)
//        //    {
//        //        string guid = context.Request.RawUrl.Remove(0, 7);
//        //        if (!MelBoxWeb.LogedInGuids.ContainsKey(guid))
//        //        {
//        //            builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Lesen des Benutzerkontos", "Bitte erneut einloggen."));
//        //            builder.Append(MelBoxWeb.HtmlFoot());

//        //            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//        //            return context;
//        //        }
//        //        else
//        //        {
//        //            logedInUserId = MelBoxWeb.LogedInGuids[guid];
//        //        }
//        //    }


//        //    builder.Append(MelBoxWeb.HtmlUnitShift(dt, logedInUserId));
//        //    builder.Append(MelBoxWeb.HtmlAccordeonInfo("Bereitschaft", "Eine Bereitschafts-Einheit geht immer über einen Tageswechsel. Ab Uhrzeit 'Beginn' bis zum Folgetag Uhrzeit 'Ende' werden eingehende Nachrichten an den eingeteilten Kontakt gesendet."));

//        //    builder.Append(MelBoxWeb.HtmlFoot());

//        //    context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//        //    return context;
//        //}

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/create")]
//        public IHttpContext CreateMelBoxShift(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//#if DEBUG
//            Console.WriteLine("/shift/create Payload:");
//            foreach (var key in args.Keys)
//            {
//                Console.WriteLine(key + ":\t" + args[key]);
//            }
//#endif
//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Neue Bereitschaft"));

//            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
//            }
//            else
//            {
//                int logedInContactId = MelBoxWeb.LogedInGuids[args["guid"]];

//                if (logedInContactId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
//                }
//                else
//                {
//                    #region Antwort verarbeiten
//                    DateTime date = DateTime.MinValue;

//                    if (args.ContainsKey("Datum") && args.ContainsKey("Beginn") && args.ContainsKey("Ende"))
//                    {

//                        int beginHour = 17;
//                        int endHour = 7;

//                        date = DateTime.Parse(args["Datum"].ToString()).Date;
//                        beginHour = int.Parse(args["Beginn"].ToString());
//                        endHour = int.Parse(args["Ende"].ToString());
//                        DateTime firstStartTime = DateTime.Now;
//                        DateTime lastEndTime = DateTime.Now;

//                        bool createWeekShift = args.ContainsKey("CreateWeekShift");

//                        if (createWeekShift)
//                        {
//                            date = date.AddDays(DayOfWeek.Monday - date.DayOfWeek);

//                            for (int i = 0; i < 7; i++)
//                            {
//                                DateTime StartTime = MelBoxSql.ShiftStandardStartTime(date);
//                                DateTime EndTime = MelBoxSql.ShiftStandardEndTime(date);

//                                if (i == 0) firstStartTime = StartTime;
//                                if (i == 6) lastEndTime = EndTime;

//                                Program.Sql.InsertShift(logedInContactId, StartTime, EndTime);
//                                date = date.AddDays(1);
//                            }
//                        }
//                        else
//                        {
//                            DateTime StartTime = date.AddHours(beginHour);
//                            DateTime EndTime = date.AddDays(1).AddHours(endHour);

//                            firstStartTime = StartTime;
//                            lastEndTime = EndTime;

//                            Program.Sql.InsertShift(logedInContactId, StartTime, EndTime);
//                        }

//                        builder.Append(MelBoxWeb.HtmlAlert(3, "Neue Bereitschaft erstellt", string.Format("Neue Bereitschaft vom {0} bis {1} erstellt.", firstStartTime, lastEndTime) ) );
//                    }
//                    else
//                    {
//                        if (args.ContainsKey("selectedRow"))
//                            if (args["selectedRow"].StartsWith("Datum_"))
//                                DateTime.TryParse(args["selectedRow"].ToString().Substring(6), out date);
//                    }

//                    #endregion

//                    builder.Append(MelBoxWeb.HtmlFormShift(date, 0, logedInContactId));

//                    Dictionary<string, string> action = new Dictionary<string, string>
//                    {
//                        { "/shift/create", "Neue Bereitschaft wirklich speichern?" }
//                    };

//                    builder.Append(MelBoxWeb.HtmlEditor(action));
//                }
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/update")]
//        public IHttpContext UpdateMelBoxShift(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//#if DEBUG
//            Console.WriteLine("/shift/update Payload:");
//            foreach (var key in args.Keys)
//            {
//                Console.WriteLine(key + ":\t" + args[key]);
//            }
//#endif
//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Bereitschaft ändern"));

//            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
//            }
//            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int shiftId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Bereitschaft zum ändern übergeben."));
//            }
//            else
//            {
//                int logedInContactId = MelBoxWeb.LogedInGuids[args["guid"]];

//                if (logedInContactId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
//                }
//                else
//                {
//                    bool isAdmin = MelBoxSql.AdminIds.Contains(logedInContactId);
//                    DateTime date = DateTime.Now.Date;

//                    #region Antwort verarbeiten
//                    if (args.ContainsKey("selectedRow") && args.ContainsKey("ContactId") && args.ContainsKey("Datum") && args.ContainsKey("Beginn") && args.ContainsKey("Ende"))
//                    {  
//                        int.TryParse(args["ContactId"].ToString(), out int contactId); ;

//                        if (contactId != logedInContactId && !isAdmin)
//                        {
//                            builder.Append(MelBoxWeb.HtmlAlert(2, "Nicht änderbar", string.Format("Sie können nur ihre eigenen Bereitschaftszeiten bearbeiten.")));
//                        }
//                        if (shiftId == 0)
//                        {
//                            builder.Append(MelBoxWeb.HtmlAlert(2, "Keine gültige Nummer", "Der gewählte Zeitraum hat keine zugewiesene Nummer oder der Aufruf war fehlerhaft. Bereitschaftszeit neu erstellen?"));
//                        }
//                        else
//                        {                           
//                            int beginHour = 17;
//                            int endHour = 7;

//                            date = DateTime.Parse(args["Datum"].ToString()).Date;
//                            beginHour = int.Parse(args["Beginn"].ToString());
//                            endHour = int.Parse(args["Ende"].ToString());
//                            string name = MelBoxWeb.DecodeUmlaute(args["Name"].ToString());

//                            //Admin: Darstellung anderer User
//                            if (contactId == 0) contactId = logedInContactId;
//                            else logedInContactId = contactId;

//                            DateTime StartTime = date.AddHours(beginHour);
//                            DateTime EndTime = date.AddDays(1).AddHours(endHour);

//                            if (!Program.Sql.UpdateShift(shiftId, contactId, StartTime, EndTime))
//                            {
//                                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Bereitschaft  Nr. " + shiftId + " bearbeiten",
//                                    string.Format("Die Bereitschaft konnte nicht in der Datenbank geändert werden.")));
//                            }
//                            else
//                            {
//                                builder.Append(MelBoxWeb.HtmlAlert(3, "Bereitschaft Nr. " + shiftId + " geändert",
//                                   string.Format("Die Bereitschaft Nr. {0} wurde geändert auf {1} im Zeitraum {2} bis {3}.", shiftId, name.Replace('+', ' '), StartTime, EndTime)));
//                            }
//                        }
//                    }
//                    #endregion

//                    builder.Append(MelBoxWeb.HtmlFormShift(date, shiftId, logedInContactId));

//                    Dictionary<string, string> action = new Dictionary<string, string>
//                    {
//                        { "/shift/update", "Diesen Eintrag wirklich ändern?" }
//                    };

//                    builder.Append(MelBoxWeb.HtmlEditor(action));
//                }
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/delete")]
//        public IHttpContext DeleteMelBoxShift(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//#if DEBUG
//            Console.WriteLine("/shift/delete Payload:");
//            foreach (var key in args.Keys)
//            {
//                Console.WriteLine(key + ":\t" + args[key]);
//            }
//#endif
//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Bereitschaft löschen"));

//            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Löschen ist nur eingelogged möglich."));
//            }
//            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int shiftId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Bereitschaft zum löschen übergeben."));
//            }
//            else
//            {
//                int logedInContactId = MelBoxWeb.LogedInGuids[args["guid"]];
//                bool isAdmin = MelBoxSql.AdminIds.Contains(logedInContactId);

//                if (logedInContactId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
//                }
//                else if (!isAdmin)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie besitzen nicht die notwendigen Rechte für diese Aktion."));
//                }
//                else
//                {
//                    if (!Program.Sql.DeleteShift(shiftId))
//                    {
//                        builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Bereitschaft  Nr. " + shiftId + " löschen",
//                            string.Format("Die Bereitschaft konnte nicht aus der Datenbank gelöscht werden.")));
//                    }
//                    else
//                    {
//                        string msg = string.Format("Die Bereitschaft Nr. {0} wurde gelöscht durch den Benutzer {1} .", shiftId, logedInContactId);

//                        builder.Append(MelBoxWeb.HtmlAlert(3, "Bereitschaft Nr. " + shiftId + " gelöscht", msg));

//                        Program.Sql.Log(MelBoxSql.LogTopic.Shift, MelBoxSql.LogPrio.Warning, msg);
//                    }
//                }                    
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        #endregion

//        #region Kontakte

//        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/account/\w+$")] //@"^/user/\d+$"
//   //     [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/account/\d+/\w+$")] //@"^/user/\d+$"
//        public IHttpContext ShowMelBoxAccount(IHttpContext context)
//        {
//            StringBuilder builder = new StringBuilder();

//            builder.Append(MelBoxWeb.HtmlHead("Benutzerkonto"));

//            int contactId = MelBoxWeb.LogedInAccountId(context.Request.RawUrl);

//            if (contactId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Lesen des Benutzerkontos", "Bitte erneut einloggen."));
//                builder.Append(MelBoxWeb.HtmlLogin());
//            }
//            else
//            {               
//                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));                
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/create")]
//        public IHttpContext CreateMelBoxAccount(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);

//            string[] args = payload.Split('&');

//            int newId = 0;
//            string name = "-KEIN NAME-";
//            string password = null;
//            int companyId = -1;
//            string email = string.Empty;
//            ulong phone = 0;
//            int sendSms = 0; //<input type='checkbox' > wird nur übertragen, wenn angehakt => immer zurücksetzten, wenn nicht gesetzt
//            int sendEmail = 0;
//            int maxInactivity = -1;

//            foreach (string arg in args)
//            {

//                if (arg.StartsWith("ContactId="))
//                {
//                    newId = int.Parse(arg.Split('=')[1]); //alte Id einlesen
//                }

//                if (arg.StartsWith("Name="))
//                {
//                    name = arg.Split('=')[1].Replace('+', ' ');
//                }

//                if (arg.StartsWith("Passwort="))
//                {
//                    password = arg.Split('=')[1];
//                }

//                if (arg.StartsWith("CompanyId="))
//                {
//                    companyId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Email="))
//                {
//                    email = arg.Split('=')[1];
//                }

//                if (arg.StartsWith("Telefon="))
//                {
//                    phone = GsmConverter.StrToPhone(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("SendSms="))
//                {
//                    string boolStr = arg.Split('=')[1];

//                    if (boolStr.ToLower() == "on")
//                        sendSms = 1;
//                    else
//                        sendSms = 0;
//                }

//                if (arg.StartsWith("SendEmail="))
//                {
//                    string boolStr = arg.Split('=')[1];

//                    if (boolStr.ToLower() == "on")
//                        sendEmail = 1;
//                    else
//                        sendEmail = 0;
//                }

//                if (arg.StartsWith("Max_Inaktivität="))
//                {
//                    maxInactivity = int.Parse(arg.Split('=')[1].ToString());
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Neuer Benutzer"));

//            if (password.Length < 4)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler - Passwort ungültig", "Das vergebene Passwort entspricht nicht den Vorgaben. Der Benutzer wird nicht erstellt."));
//            }
//            else
//            {
//                newId = Program.Sql.InsertContact(name, password, companyId, email, phone, maxInactivity, sendSms == 1, sendEmail == 1);
//                if (newId == 0)
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Schreiben in die Datenbank", "Der Benutzer '" + name + "' konnte nicht erstellt werden."));
//                }
//                else
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' erstellt", "Der Benutzer '" + name + "' wurde in der Datenbank neu erstellt."));
//                }
//            }

//            if (newId != 0)
//            {
//                builder.Append(MelBoxWeb.HtmlUnitAccount(newId));
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/update")]
//        public IHttpContext UpdateMelBoxAccount(IHttpContext context)
//#pragma warning restore CA1822 // Mark members as static
//        {          
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            //Console.WriteLine("\r\n2: account/update payload:\r\n" + payload);

//            string[] args = payload.Split('&');

//            int contactId = 0;
//            string name = "-KEIN NAME-";
//            string password = null;
//            int companyId = -1;
//            string email = null;
//            ulong phone = 0;
//            int sendSms = 0; //<input type='checkbox' > wird nur übertragen, wenn angehakt => immer zurücksetzten, wenn nicht gesetzt
//            int sendEmail = 0;
//            int maxInactivity = -1;

//            foreach (string arg in args)
//            {
//                if (arg.StartsWith("ContactId="))
//                {
//                    contactId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Name="))
//                {
//                    name = arg.Split('=')[1].Replace('+', ' ');
//                }

//                if (arg.StartsWith("Passwort="))
//                {
//                    password = arg.Split('=')[1];
//                }

//                if (arg.StartsWith("CompanyId="))
//                {
//                    companyId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Email="))
//                {
//                    email = arg.Split('=')[1];
//                }

//                if (arg.StartsWith("Telefon="))
//                {
//                    phone = GsmConverter.StrToPhone(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("SendSms="))
//                {
//                    string boolStr = arg.Split('=')[1];

//                    if (boolStr.ToLower() == "on")
//                        sendSms = 1;
//                    else
//                        sendSms = 0;
//                }

//                if (arg.StartsWith("SendEmail="))
//                {
//                    string boolStr = arg.Split('=')[1];

//                    if (boolStr.ToLower() == "on")
//                        sendEmail = 1;
//                    else
//                        sendEmail = 0;
//                }

//                if (arg.StartsWith("Max_Inaktivität="))
//                {
//                    maxInactivity = int.Parse( arg.Split('=')[1].ToString() );
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Änderung Benutzer"));

//            if (contactId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Änderungen für Benutzer '" + name + "'", "Der Aufruf war fehlerhaft."));
//                builder.Append(MelBoxWeb.HtmlFoot());
//            }
//            else
//            {
//                if (!Program.Sql.UpdateContact(contactId, name, password, companyId, phone, sendSms, email, sendEmail, string.Empty, maxInactivity))
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Änderungen für Benutzer '" + name + "'", "Die Änderungen konnten nicht in die Datenbank übertragen werden."));
//                }
//                else
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(3, "Änderungen für Benutzer '" + name + "' übernommen", "Die Änderungen an Benutzer '" + name + "' wurden in der Datenbank gespeichert."));
//                }

//                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());
//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/delete")]
//        public IHttpContext DeleteMelBoxAccount(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);

//            string[] args = payload.Split('&');

//            int contactId = 0;
//            string name = "-KEIN NAME-";

//            foreach (string arg in args)
//            {
//                if (arg.StartsWith("ContactId="))
//                {
//                    contactId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Name="))
//                {
//                    name = arg.Split('=')[1].Replace('+', ' ');
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Lösche Benutzer"));

//            if (contactId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler", "Der Benutzer '" + name + "' konnte nicht gelöscht werden. Der Aufruf war fehlerhaft."));
//            }
//            else
//            {
//                if (!Program.Sql.DeleteContact(contactId) )
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler beim löschen von '" + name + "'", "Der Benutzer " + contactId + " '" + name + "' konnte nicht aus der Datenbank gelöscht werden."));
//                }
//                else
//                {
//                    builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' gelöscht", "Der Benutzer " + contactId + " '" + name + "' wurde aus der Datenbank gelöscht."));
//                }

//                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        #endregion

//        #region Firmen

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company")]
//        //[RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/company/\w+$")]
//        public IHttpContext ShowMelBoxCompany(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//#if DEBUG
//            Console.WriteLine("/shift/create Payload:");
//            foreach (var key in args.Keys)
//            {
//                Console.WriteLine(key + ":\t" + args[key]);
//            }
//#endif
//            int companyId =  0;
//            int logedInUserId = 0;

//            foreach (string arg in args.Keys)
//            {
//                switch (arg)
//                {
//                    case "companyId":
//                        int.TryParse(args[arg], out companyId);
//                        break;
//                    case "guid":
//                        if (MelBoxWeb.LogedInGuids.ContainsKey(args[arg]))
//                        {
//                            logedInUserId = MelBoxWeb.LogedInGuids[args[arg]];
//                        }
//                        break;
//                    default:
//                        break;
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Firmeninformationen"));

//            if (logedInUserId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Keine Berechtigung", "Die Seite wurde mit ungültigen Parametern aufgerufen."));
//            }
//            else if (companyId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die Seite wurde mit ungültigen Parametern aufgerufen."));                
//            }
//            else
//            {
//                builder.Append(MelBoxWeb.HtmlUnitCompany(companyId, logedInUserId));
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/create")]
//        public IHttpContext AddMelBoxCompany(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//           // payload = MelBoxWeb.DecodeUmlaute(payload);
//            Console.WriteLine("\r\n company/create payload:\r\n" + payload);

//            string[] args = payload.Split('&');
//            string name = string.Empty;
//            string address = string.Empty;
//            string city = string.Empty;

//            foreach (string arg in args)
//            {
//                if (arg.StartsWith("Name="))
//                {
//                    name = MelBoxWeb.DecodeUmlaute(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Adresse="))
//                {
//                    address = MelBoxWeb.DecodeUmlaute(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Ort="))
//                {
//                    city = MelBoxWeb.DecodeUmlaute(arg.Split('=')[1]);
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Änderung Firmendaten"));

//            if (name.Length < 3)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(2, "Kein neuer Firmeneintrag", "Der Firmenname muss mindestens 3 Zeichen lang sein."));
//            }
//            else if (!Program.Sql.InsertCompany(name, address, city))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler neuer Firmeneintrag", "'" + name + "', '" + address + "', '" + city + "'<br>" +
//                    "Es konnte kein neuer Eintrag in die Datenbank geschrieben werden."));
//            }
//            else
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(3, "Neuer Firmeneintrag", "Die Firma <p>'" + name + "'<br>'" + address + "'<br>'" + city + "'</p>" +
//                        "wurde erfolgreich in die Datenbank aufgenommen."));
//            }

//            //Dictionary<string, string> action = new Dictionary<string, string>
//            //    {
//            //        { "/company/create", "Firma neu anlegen?" },
//            //        { "/company/update", "Wirklich speichern?" }
//            //    };

//            #region Firma anzeigen

//            int lastId = Program.Sql.GetLastCompany();

//            builder.Append(MelBoxWeb.HtmlUnitCompany(lastId));
//           // builder.Append(MelBoxWeb.HtmlEditor(action));
//            builder.Append(MelBoxWeb.HtmlFoot());
//            #endregion

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/update")]
//        public IHttpContext UpdateMelBoxCompany(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//           // Console.WriteLine("\r\n company/update payload:\r\n" + payload);

//            string[] args = payload.Split('&');
//            int companyId = 0;
//            string name = string.Empty;
//            string address = string.Empty;
//            string city = string.Empty;

//            foreach (string arg in args)
//            {
//                if (arg.StartsWith("Id="))
//                {
//                    companyId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Name="))
//                {
//                    name = arg.Split('=')[1].Replace('+', ' ');
//                }

//                if (arg.StartsWith("Adresse="))
//                {
//                    address = arg.Split('=')[1].Replace('+', ' ');
//                }

//                if (arg.StartsWith("Ort="))
//                {
//                    city = arg.Split('=')[1].Replace('+', ' ');
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Änderung Firmendaten"));

//            if (0 == Program.Sql.UpdateCompany(companyId, name, address, city))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine &Auml;nderungen für Firma '" + name + "'", "Es wurden keine Änderungen übergeben oder der Aufruf war fehlerhaft."));
//            }
//            else
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(3, "Änderungen für Firma '" + name + "' gespeichert", "Die Änderungen an Firma '" + name + "' wurden in der Datenbank gespeichert."));
//            }

//            if (companyId == 0)
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Aufruffehler", "Der Aufruf dieser Seite war fehlerhaft."));                
//            }
//            else
//            {
//                builder.Append(MelBoxWeb.HtmlUnitCompany(companyId));
//            }

//            //Dictionary<string, string> action = new Dictionary<string, string>
//            //    {
//            //        { "/company/create", "Firma neu anlegen?" },
//            //        { "/company/update", "Wirklich speichern?" }
//            //    };

//            //#region Firma anzeigen
//            //DataTable dtCompany = Program.Sql.GetAllCompanys();
//            //builder.Append(MelBoxWeb.HtmlFormCompany(dtCompany, companyId));
//            //builder.Append(MelBoxWeb.HtmlEditor(action));
//            //#endregion

//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/delete")]
//        public IHttpContext DeleteMelBoxCompany(IHttpContext context)
//        {
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            // Console.WriteLine("\r\n company/update payload:\r\n" + payload);

//            string[] args = payload.Split('&');
//            int companyId = 0;
//            string name = "-KEIN NAME-";

//            foreach (string arg in args)
//            {
//                if (arg.StartsWith("Id="))
//                {
//                    companyId = int.Parse(arg.Split('=')[1]);
//                }

//                if (arg.StartsWith("Name="))
//                {
//                    name = arg.Split('=')[1].Replace('+', ' ');
//                }
//            }

//            StringBuilder builder = new StringBuilder();
//            builder.Append(MelBoxWeb.HtmlHead("Änderung Firmendaten"));

//            if (!Program.Sql.DeleteCompany(companyId))
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(1, "Keine &Auml;nderungen für Firma '" + name + "'", "Es wurden keine Änderungen übergeben oder der Aufruf war fehlerhaft."));
//            }
//            else
//            {
//                builder.Append(MelBoxWeb.HtmlAlert(3, "Firma '" + name + "' gelöscht", "Die Firma '" + name + "' wurde aus der Datenbank gelöscht."));
//            }

//            builder.Append(MelBoxWeb.HtmlFoot());

//            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
//            return context;
//        }

//        #endregion

//        [RestRoute]
//        public IHttpContext Home(IHttpContext context)
//        {
//            #region Payload
//            string payload = context.Request.Payload;
//            payload = MelBoxWeb.DecodeUmlaute(payload);
//            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

//            string guid = string.Empty;
//            string alert = string.Empty;


//            if (args.ContainsKey("name") && args.ContainsKey("password"))
//            {
//                string name = args["name"].Replace('+', ' ');
//                string password = args["password"];

//                int myId = Program.Sql.GetContactIdFromLogin(name, password);

//                if (myId != 0) //Es wurde ein Benutzer mit diesen Zugangsdaten gefunden
//                {
//                    Program.Sql.Log(MelBoxSql.LogTopic.Sql, MelBoxSql.LogPrio.Info, "Benutzer " + myId + " '" + name + "' ist angemeledt.");

//                    guid = MelBoxWeb.GenerateID(myId);
//                    alert = MelBoxWeb.HtmlAlert(3, "Login erfolgreich", string.Format("Sie haben sich erfolgreich als Benutzer {0} '" + name + "' mit der ID {1} eingeloggt.", myId, guid));
//                }
//                else
//                {
//                    alert = MelBoxWeb.HtmlAlert(2, "Login fehlgeschlagen", string.Format("Der Benutzername oder das Passwort sind unbekannt.", myId, guid));
//                }
//            }
/*
             #endregion
     /*
 //            StringBuilder builder = new StringBuilder();

 //            builder.Append(MelBoxWeb.HtmlHead("St&ouml;rmeldesystem f&uuml;r Kreutztr&auml;ger K&auml;ltetechnik", guid));
 //            builder.Append(alert);
 //            builder.Append(MelBoxWeb.HtmlLogin());

 //            #region TEST
 //#if DEBUG
 //            if (MelBoxWeb.LogedInGuids.Count > 0)
 //            {
 //                builder.Append("<p>LogedIn</p> <ul class='w3-ul'>\n");

 //                foreach (var item in MelBoxWeb.LogedInGuids.Keys)
 //                {
 //                    builder.Append("<li>" + MelBoxWeb.LogedInGuids[item] + "\t" + item + "</li>\n");
 //                }
 //                builder.Append("</ul>\n");
 //            }

 //            builder.Append("<p>RawUrl: " + context.Request.RawUrl + "</p>");
 //#endif
 //            #endregion

 //            builder.Append(MelBoxWeb.HtmlFoot());

 //            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
 //            return context;
 //        }
 //    }

}
*/

#endregion
