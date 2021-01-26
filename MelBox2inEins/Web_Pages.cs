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
           // Console.WriteLine("Aufruf von {0} durch: [{1}] {2} - Admin: {3} | {4}", requestingPage, logedInUserId, logedInUserName, isAdmin, guid);
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

        #region Nachrichten

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

            builder.Append(MelBoxWeb.HtmlTablePlain(dt, isAdmin));
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

            builder.Append(MelBoxWeb.HtmlTablePlain(dt, false));
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

            if (dt.Rows.Count == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(3, "Keine Zeitüberschreitungen festgestellt", "Zur Zeit ist kein überwachter Sender in Verzug."));
                
                dt = Program.Sql.GetMonitoredContactList();
                builder.Append(MelBoxWeb.HtmlTablePlain(dt, false));
            }
            else
            {
                builder.Append(MelBoxWeb.HtmlTablePlain(dt, false));
            }

            const string info = "Den einzelnen Benutzern kann ein Wert 'Max_Inaktiv' [in Stunden] zugewiesen werden. " +
                "Kommt von diesen Benutzern innherhalb der eingestellten Zeit keine Meldung, sollte der Meldeweg (SMS, Email) geprüft werden.";

            builder.Append(MelBoxWeb.HtmlInfoSidebar("Überwachte Meldungen", info));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        #endregion

        #region Gesperrte Nachrichten

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

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/create")]
        public IHttpContext ResponseBlockedCreate(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            #region Inhalt ermitteln
            int recMsgId = MelBoxWeb.GetArgInt(args, "selectedRow");

            int contentId = Program.Sql.GetContentId(recMsgId);
            #endregion

            StringBuilder builder = new StringBuilder();

            if (contentId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
            }
            else
            {
                Program.Sql.InsertMessageBlocked(contentId);
                string msg = "Die Weiterleitung der Nachricht Nr. " + contentId + " wurde durch den Benutzer '" + logedInUserName + "' zu bestimmten Zeiten gesperrt.";

                builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht in die Sperrliste aufgenommen", msg));

                Program.Sql.Log(MelBoxSql.LogTopic.Shift, MelBoxSql.LogPrio.Warning, msg);
            }

            Dictionary<string, string> action = new Dictionary<string, string>();

            if (isAdmin)
            {
                action.Add("/blocked/update", "Gesperrte Nachricht bearbeiten");
                action.Add("/blocked/delete", "Aus Sperrliste entfernen");
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt, 0, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/update")]
        public IHttpContext ResponseBlockedUpdate(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            #region Inhalt ermitteln
            int recMsgId = MelBoxWeb.GetArgInt(args, "selectedRow");
            int beginHour = MelBoxWeb.GetArgInt(args, "Beginn");
            int endHour = MelBoxWeb.GetArgInt(args, "Ende");
            int contentId = recMsgId;
            #endregion

            StringBuilder builder = new StringBuilder();

            if (args.ContainsKey("Beginn") && args.ContainsKey("Ende"))
            {
                //int beginHour = int.Parse(args["Beginn"].ToString());
                //int endHour = int.Parse(args["Ende"].ToString());

                MelBoxSql.BlockedDays days = MelBoxSql.BlockedDays.None;
                if (args.ContainsKey("Mo")) days |= MelBoxSql.BlockedDays.Monday;
                if (args.ContainsKey("Di")) days |= MelBoxSql.BlockedDays.Tuesday;
                if (args.ContainsKey("Mi")) days |= MelBoxSql.BlockedDays.Wendsday;
                if (args.ContainsKey("Do")) days |= MelBoxSql.BlockedDays.Thursday;
                if (args.ContainsKey("Fr")) days |= MelBoxSql.BlockedDays.Friday;
                if (args.ContainsKey("Sa")) days |= MelBoxSql.BlockedDays.Saturday;
                if (args.ContainsKey("So")) days |= MelBoxSql.BlockedDays.Sunday;

                if (Program.Sql.UpdateMessageBlocked(contentId, beginHour, endHour, days))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Sperrzeiten geändert", "Die Sperrzeiten für Nachricht Nr. " + contentId + " wurden geändert."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Sperrzeiten nicht geändert", "Die Sperrzeiten für Nachricht Nr. " + contentId + " konnten nicht geändert werden."));
                }
            }
        
            Dictionary<string, string> action = new Dictionary<string, string>();

            if (isAdmin)
            {
                action.Add("/blocked/update", "Änderungen speichern");
                action.Add("/blocked/delete", "Aus Sperrliste entfernen");
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt, contentId, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/delete")]
        public IHttpContext ResponseBlockedDelete(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            #region Inhalt ermitteln
            int contentId = 0;
            if (args.ContainsKey("selectedRow"))
            {
                int.TryParse(args["selectedRow"].ToString(), out contentId);
            }
            #endregion

            StringBuilder builder = new StringBuilder();

            if (contentId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
            }
            else
            {
                if (!Program.Sql.DeleteMessageBlocked(contentId))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler Nachricht entsperren", "Die Nachricht mit der Nr. " + contentId + " konnte nicht aus der Sperrliste freigegeben werden."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht aus der Sperrliste genommen", "Die gesperrte Nachricht mit der Nr. " + contentId + " wurde durch '" + logedInUserName + "' wieder freigegeben."));
                }
            }

            Dictionary<string, string> action = new Dictionary<string, string>();

            if (isAdmin)
            {
                action.Add("/blocked/update", "Gesperrte Nachricht bearbeiten");
                action.Add("/blocked/delete", "Aus Sperrliste entfernen");
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlTableBlocked(dt, 0, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        #endregion

        #region Bereitschaft

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift")]
        public IHttpContext ResponseShift(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            #region Tabelle anzeigen
            Dictionary<string, string> action = new Dictionary<string, string>
            {
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
            #endregion
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
            ReadGlobalFields(args);

            int shiftId = 0;
            DateTime date = DateTime.MinValue;
            int shiftUserId = logedInUserId;
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

            if (args.ContainsKey("ContactId"))
            {
                int.TryParse(args["ContactId"].ToString(), out shiftUserId);
            }

            builder.Append(MelBoxWeb.HtmlFormShift(date, shiftId, shiftUserId, isAdmin));

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

            #region Tabelle anzeigen
            Dictionary<string, string> action = new Dictionary<string, string>
            {
                { "/shift/edit", "Bereitschaft bearbeiten" }
            };

            if (isAdmin)
            {
                action.Add("/shift/delete", "Bereitschaft löschen");
            }

            DataTable dt = Program.Sql.GetViewShift();
            builder.Append(MelBoxWeb.HtmlTableShift(dt, 0, logedInUserId, isAdmin));
            builder.Append(MelBoxWeb.HtmlEditor(action));
            #endregion
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

        #endregion

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
                builder.Append(MelBoxWeb.HtmlAlert(2, "Ungültiger Aufruf", "Für Einsicht in das Benutzerkonto bitte einloggen."));
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

            if (logedInUserId != 0)
            {
                int chosenContactId = MelBoxWeb.GetArgInt(args, "ContactId");

                DataTable dt;
                if (isAdmin) dt = Program.Sql.GetViewContactInfo();
                else dt = Program.Sql.GetViewContactInfo(chosenContactId);

                DataTable dtCompany = Program.Sql.GetCompany();

                builder.Append(MelBoxWeb.HtmlFormAccount(dt, dtCompany, chosenContactId, isAdmin));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Benutzerkonto anlegen", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/update")]
        public IHttpContext ResponseAccountUpdate(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();
            int chosenContactId = MelBoxWeb.GetArgInt(args, "ContactId");

            if (!isAdmin && chosenContactId != logedInUserId)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Benutzer zu ändern."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormAccount(args, false));
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
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Benutzerkonto ändern", logedInUserName));
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

        #region Firmeninformationen
       
        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company")]
        public IHttpContext ResponseCompany(IHttpContext context)
        {
            string caption = "Firmenkonto";
            int companyId = 0;
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            if (args.ContainsKey("CompanyId"))
            {
                int.TryParse(args["CompanyId"].ToString(), out companyId);
            }

            //Dictionary<string, string> action = new Dictionary<string, string>();
            //if (isAdmin)
            //{
            //    action.Add("/company/create", "Firma neu anlegen");
            //    action.Add("/company/update", "Firmeninformationen ändern");
            //    action.Add("/company/delete", "Firma löschen");
            //}

            DataTable dtCompany = Program.Sql.GetCompany();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlFormCompany(dtCompany, companyId, isAdmin));
            //builder.Append(MelBoxWeb.HtmlEditor(action));

            int chosenContactId = logedInUserId;
            if (requestingPage == caption && args.ContainsKey("selectedContactId")) //Ist Antwort von dieser Seite
            {
                int.TryParse(args["selectedContactId"].ToString(), out chosenContactId);
            }
            else if (chosenContactId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Ungültiger Aufruf", "Für Einsicht Benutzerkonto bitte einloggen."));
            }

#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), caption, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/create")]
        public IHttpContext ResponseCompanyCreate(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung einen Firmeneintrag zu erstellen."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormCompany(args, true));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Firmenkonto erstellen", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/update")]
        public IHttpContext ResponseCompanyUpdate(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Firmeneintrag zu ändern."));
            }
            else
            {
                builder.Append(MelBoxWeb.ProcessFormCompany(args, false));
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Firmenkonto ändern", logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/delete")]
        public IHttpContext ResponseCompanyDelete(IHttpContext context)
        {
            string payload = context.Request.Payload;

            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (!isAdmin)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Berechtigung", "Sie haben keine Berechtigung den Firmeneintrag zu löschen."));
            }
            else
            {
                int companyId = MelBoxWeb.GetArgInt(args, "CompanyId");
                if (companyId != 0)
                {
                    string name = MelBoxWeb.GetArgStr(args, "Name");//.Replace('+', ' ');
                    if (!Program.Sql.DeleteCompany(companyId))
                    {                        
                        builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler beim Löschen von Firma '" + name + "'", "Die Firma '" + name + "' konnte nicht aus der Datenbank gelöscht werden."));
                    }
                    else
                    {
                        builder.Append(MelBoxWeb.HtmlAlert(3, "Firma '" + name + "'gelöscht", "Die Firma '" + name + "' wurde aus der Datenbank gelöscht."));
                    }
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Ungültiger Aufruf", "Die Firmeninformationen konnten nicht zugewiesen werden."));
                }
            }
#if DEBUG
            builder.Append("<p class='w3-pink w3-mobile'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Firmenkonto löschen", logedInUserName));
            return context;
        }

        #endregion

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/log")]
        public IHttpContext ResponseLog(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            DateTime von = DateTime.UtcNow.AddDays(-2);
            DateTime bis = DateTime.UtcNow;

            string vonStr = MelBoxWeb.GetArgStr(args, "von");
            string bisStr = MelBoxWeb.GetArgStr(args, "bis");

            if (vonStr.Length > 9)
                DateTime.TryParse(vonStr, out von);

            if (bisStr.Length > 9)
                DateTime.TryParse(bisStr, out bis);

            DataTable dt = Program.Sql.GetViewLog(von, bis.AddDays(1));

            StringBuilder builder = new StringBuilder();


            builder.Append(MelBoxWeb.HtmlFormLog(von, bis));
            builder.Append(MelBoxWeb.HtmlTablePlain(dt, false));

#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), dt.TableName, logedInUserName));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/register")]
        public IHttpContext ResponseRegister(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlAlert(4, "Registrierung", "Anfragen zur Registrierung gehen an den Administrator, der sie händisch freigeschaltet. <br>Bei Anfrage bitte den gewünschten Nutzernamen angeben. <br>Nach Freischaltung bitte selbst unter 'Account' ein Passwort vergeben."));
            builder.Append(MelBoxWeb.HtmlAlert(2, "Bitte beachten", "Es ist keine Registrierungs-Prozedur implementiert. <a href='mailto:harm.schnakenberg@kreutztraeger.de'>Email</a>"));

#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Registrierung", logedInUserName));
            return context;
        }


        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/gsm")]
        public IHttpContext ResponseGsm(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

            ReadGlobalFields(args);
             
            StringBuilder builder = new StringBuilder();

            #region Tabelle füllen
            DataTable dt = new DataTable();
            dt.Columns.Add("Parameter", typeof(string));
            dt.Columns.Add("Wert", typeof(string));

            string para = "Verbunden mit GSM-Modem";
            string value = GlobalProperty.ConnectedToModem ? "verbunden" : "keine Verbindung";
            dt.Rows.Add(para, value);

            para = "SIM-Schubfach erkannt";
            value = GlobalProperty.SimHolderDetected ? "erkannt" : "nicht erkannt";
            dt.Rows.Add(para, value);

            para = "Eigene Telefonnummer";
            value = "+" + GlobalProperty.OwnPhone;
            dt.Rows.Add(para, value);

            para = "Registrierung Mobilfunknetz";
            value = GlobalProperty.NetworkRegistrationStatus;
            dt.Rows.Add(para, value);

            para = "Mobilfunknetz Empfangsqualität";
            value = string.Format("{0:0}%", GlobalProperty.GsmSignalQuality); 
            dt.Rows.Add(para, value);

            para = "Fehlende Empfangsbestätigungen";
            value = string.Format("{0}", GlobalProperty.SmsPendingReports);
            dt.Rows.Add(para, value);

            para = "SMS-Text für Meldeweg Test";
            value = GlobalProperty.SmsRouteTestTrigger;
            dt.Rows.Add(para, value);

            para = "Zuletzt gesendete SMS";
            value = GlobalProperty.LastSmsSend;
            dt.Rows.Add(para, value);
            #endregion

            builder.Append(MelBoxWeb.HtmlTablePlain(dt, false));
#if DEBUG
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), "Status Modem", logedInUserName));
            return context;
        }


        [RestRoute]
        public IHttpContext Login(IHttpContext context)
        {
            string caption = "Login"; //Überschriftd er Seite
            string newGuid = string.Empty; //Nur füllen, wenn neue Benutzeranmeldung
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);
            ReadGlobalFields(args);

            StringBuilder builder = new StringBuilder();

            if (args.ContainsKey("name") && args.ContainsKey("password"))
            {
                string name = args["name"]; //.Replace('+', ' ');
                string password = args["password"];

                newGuid = MelBoxWeb.CheckLogin(name, password);

                if (newGuid.Length == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Login fehlgeschlagen", "Login für Benutzer '" + name + "' fehlgeschlagen.<br>Benutzer und Passwort korrekt?"));
                }
                else
                {                   
                    User newLogedInUser = MelBoxWeb.GetUserFromGuid(newGuid);
                    guid = newGuid;
                    logedInUserName = newLogedInUser.Name;
                    logedInUserId = newLogedInUser.Id;
                    isAdmin = newLogedInUser.IsAdmin;
                }
            }

            if (guid.Length == 0 || logedInUserId == 0)
            {
                builder.Append(MelBoxWeb.HtmlLogin());
            }
            else
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, isAdmin ? "Angemeldet als Administrator" : "Angemeldet als Benutzer", string.Format("[{0}] {1}", logedInUserId, logedInUserName)));
                builder.Append(MelBoxWeb.HtmlLogout());
            }
#if DEBUG           
            builder.Append("<p class='w3-pink'>" + payload + "</p>");
#endif
            context.Response.SendResponse(MelBoxWeb.HtmlCanvas(builder.ToString(), caption, logedInUserName, newGuid));
            return context;
        }
    }
}
