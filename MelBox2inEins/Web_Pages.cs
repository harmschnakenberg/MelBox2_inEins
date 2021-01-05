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
        #region nicht änderbare Tabellen
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/log")]
        public IHttpContext ShowMelBoxLog(IHttpContext context)
#pragma warning restore CA1822 // Mark members as static
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
            Dictionary<string, string> action = new Dictionary<string, string>
            {
                { "/blocked/create", "Ausgewählte Nachricht sperren" }
            };

            try
            {
                DataTable dt = Program.Sql.GetViewMsgRec();
                builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
                builder.Append(MelBoxWeb.HtmlTablePlain(dt, true));
                builder.Append(MelBoxWeb.HtmlEditor(action));
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

        #endregion

        #region Gesperrte Nachrichten
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/blocked")]
        public IHttpContext ShowMelBoxBlocked(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewMsgBlocked();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt) );
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/create")]
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
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht in die Sperrliste aufgenommen", "Die Nachricht mit der Nr. " + contentId + " wird nicht mehr in die Bereitschaft weitergeleitet."));
                }
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();

            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt));
            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/update")]
        public IHttpContext UpdateMelBoxBlocked(IHttpContext context)
        {
            string payload = context.Request.Payload;
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);
#if DEBUG
            Console.WriteLine("/blocked/update Payload:");
            foreach (var key in args.Keys)
            {
                Console.WriteLine(key + ":\t" + args[key]);
            }
#endif
            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Sperrzeiten ändern"));

            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
            }
            else if (!args.ContainsKey("selectedRow") || !int.TryParse(args["selectedRow"], out int contentId))
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Es wurde keine gültige Nachricht zum ändern übergeben."));
            }
            else
            {
                if (contentId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die übergebene Nachricht konnte nicht zugeordnet werden."));
                }
                else
                {
                    if (args.ContainsKey("Beginn") && args.ContainsKey("Ende"))
                    {
                        int beginHour = int.Parse(args["Beginn"].ToString());
                        int endHour = int.Parse(args["Ende"].ToString());

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
                }
                //nur ausgwählte Nachricht anzeigen
                DataTable dt = Program.Sql.GetViewMsgBlocked();
                builder.Append(MelBoxWeb.HtmlUnitBlocked(dt, contentId));
            }

            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/blocked/delete")]
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
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Nachricht aus der Sperrliste genommen", "Die Nachricht mit der Nr. " + contentId + " wird wieder in die Bereitschaft weitergeleitet."));
                }
            }

            DataTable dt = Program.Sql.GetViewMsgBlocked();
            builder.Append(MelBoxWeb.HtmlUnitBlocked(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        #endregion

        #region Bereitschaft
        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/shift")]
        public IHttpContext ShowMelBoxShift(IHttpContext context)
        {
            DataTable dt = Program.Sql.GetViewShift();

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead(dt.TableName));
            builder.Append(MelBoxWeb.HtmlUnitShift(dt));
            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/shift/create")]
        public IHttpContext CreateMelBoxShift(IHttpContext context)
        {
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
            Dictionary<string, string> args = MelBoxWeb.ReadPayload(payload);

#if DEBUG
            Console.WriteLine("/shift/create Payload:");
            foreach (var key in args.Keys)
            {
                Console.WriteLine(key + ":\t" + args[key]);
            }
#endif
            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Neue Bereitschaft"));

            if (!MelBoxWeb.LogedInGuids.ContainsKey(args["guid"]))
            {
                builder.Append(MelBoxWeb.HtmlAlert(4, "Bitte einloggen", "Änderungen sind nur eingelogged möglich."));
            }
            else
            {
                int contactId = MelBoxWeb.LogedInGuids[args["guid"]];

                if (contactId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Der eingeloggte Benutzer konnte nicht zugeordnet werden."));
                }
                else
                {                   
                    builder.Append(MelBoxWeb.HtmlFormShift(0,contactId));
                    //BAUSTELE


                }
            }

            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        #endregion

        #region Kontakte

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/account/\w+$")] //@"^/user/\d+$"
        public IHttpContext ShowMelBoxAccount(IHttpContext context)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlHead("Benutzerkonto"));

            string guid = context.Request.RawUrl.Remove(0, 9);
          
            if (!MelBoxWeb.LogedInGuids.ContainsKey(guid))
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Lesen des Benutzerkontos", "Bitte erneut einloggen."));
            }
            else
            {
                int contactId = MelBoxWeb.LogedInGuids[guid];
                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));                
            }

            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/create")]
        public IHttpContext CreateMelBoxAccount(IHttpContext context)
        {
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);

            string[] args = payload.Split('&');

            int newId = 0;
            string name = "-KEIN NAME-";
            string password = null;
            int companyId = -1;
            string email = string.Empty;
            ulong phone = 0;
            int sendSms = 0; //<input type='checkbox' > wird nur übertragen, wenn angehakt => immer zurücksetzten, wenn nicht gesetzt
            int sendEmail = 0;
            int maxInactivity = -1;

            foreach (string arg in args)
            {

                if (arg.StartsWith("ContactId="))
                {
                    newId = int.Parse(arg.Split('=')[1]); //alte Id einlesen
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

                if (arg.StartsWith("SendSms="))
                {
                    string boolStr = arg.Split('=')[1];

                    if (boolStr.ToLower() == "on")
                        sendSms = 1;
                    else
                        sendSms = 0;
                }

                if (arg.StartsWith("SendEmail="))
                {
                    string boolStr = arg.Split('=')[1];

                    if (boolStr.ToLower() == "on")
                        sendEmail = 1;
                    else
                        sendEmail = 0;
                }

                if (arg.StartsWith("Max_Inaktivität="))
                {
                    maxInactivity = int.Parse(arg.Split('=')[1].ToString());
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Neuer Benutzer"));

            if (password.Length < 4)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler - Passwort ungültig", "Das vergebene Passwort entspricht nicht den Vorgaben. Der Benutzer wird nicht erstellt."));
            }
            else
            {
                newId = Program.Sql.InsertContact(name, password, companyId, email, phone, maxInactivity, sendSms == 1, sendEmail == 1);
                if (newId == 0)
                {
                    builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler beim Schreiben in die Datenbank", "Der Benutzer '" + name + "' konnte nicht erstellt werden."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' erstellt", "Der Benutzer '" + name + "' wurde in der Datenbank neu erstellt."));
                }
            }

            if (newId != 0)
            {
                builder.Append(MelBoxWeb.HtmlUnitAccount(newId));
            }

            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/update")]
        public IHttpContext UpdateMelBoxAccount(IHttpContext context)
#pragma warning restore CA1822 // Mark members as static
        {          
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
            //Console.WriteLine("\r\n2: account/update payload:\r\n" + payload);

            string[] args = payload.Split('&');

            int contactId = 0;
            string name = "-KEIN NAME-";
            string password = null;
            int companyId = -1;
            string email = null;
            ulong phone = 0;
            int sendSms = 0; //<input type='checkbox' > wird nur übertragen, wenn angehakt => immer zurücksetzten, wenn nicht gesetzt
            int sendEmail = 0;
            int maxInactivity = -1;

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

                if (arg.StartsWith("SendSms="))
                {
                    string boolStr = arg.Split('=')[1];

                    if (boolStr.ToLower() == "on")
                        sendSms = 1;
                    else
                        sendSms = 0;
                }

                if (arg.StartsWith("SendEmail="))
                {
                    string boolStr = arg.Split('=')[1];

                    if (boolStr.ToLower() == "on")
                        sendEmail = 1;
                    else
                        sendEmail = 0;
                }

                if (arg.StartsWith("Max_Inaktivität="))
                {
                    maxInactivity = int.Parse( arg.Split('=')[1].ToString() );
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Änderung Benutzer"));

            if (contactId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Änderungen für Benutzer '" + name + "'", "Der Aufruf war fehlerhaft."));
                builder.Append(MelBoxWeb.HtmlFoot());
            }
            else
            {
                if (0 == Program.Sql.UpdateContact(contactId, name, password, companyId, phone, sendSms, email, sendEmail, string.Empty, maxInactivity))
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Keine Änderungen für Benutzer '" + name + "'", "Die Änderungen konnten nicht in die Datenbank übertragen werden."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Änderungen für Benutzer '" + name + "' übernommen", "Die Änderungen an Benutzer '" + name + "' wurden in der Datenbank gespeichert."));
                }

                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));
            }

            builder.Append(MelBoxWeb.HtmlFoot());
            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/account/delete")]
        public IHttpContext DeleteMelBoxAccount(IHttpContext context)
        {
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
          
            string[] args = payload.Split('&');

            int contactId = 0;
            string name = "-KEIN NAME-";
          
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
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Lösche Benutzer"));

            if (contactId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler", "Der Benutzer '" + name + "' konnte nicht gelöscht werden. Der Aufruf war fehlerhaft."));
            }
            else
            {
                if (0 == Program.Sql.DeleteContact(contactId) )
                {
                    builder.Append(MelBoxWeb.HtmlAlert(2, "Fehler beim löschen von '" + name + "'", "Der Benutzer " + contactId + " '" + name + "' konnte nicht aus der Datenbank gelöscht werden."));
                }
                else
                {
                    builder.Append(MelBoxWeb.HtmlAlert(3, "Benutzer '" + name + "' gelöscht", "Der Benutzer " + contactId + " '" + name + "' wurde aus der Datenbank gelöscht."));
                }

                builder.Append(MelBoxWeb.HtmlUnitAccount(contactId));
            }

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }


        #endregion

        #region Firmen

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = @"^/company/\w+$")]
        public IHttpContext ShowMelBoxCompany(IHttpContext context)
        {
            string queryString = context.Request.RawUrl.Remove(0, 9); //context.Request.QueryString["companyId"] ?? "1";

            StringBuilder builder = new StringBuilder();

            builder.Append(MelBoxWeb.HtmlHead("Firmeninformationen"));

            builder.Append("<p>" + context.Request.RawUrl.Remove(0, 9) + "</p>");

            if (!int.TryParse(queryString, out int companyId))
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler", "Die Seite wurde mit ungültigen Parametern aufgerufen.<br>" + context.Request.RawUrl.Remove(0, 9)));
                
            }
            else
            {
                builder.Append(MelBoxWeb.HtmlUnitCompany(companyId));

                //Dictionary<string, string> action = new Dictionary<string, string>
                //{
                //    { "/company/create", "Firma neu anlegen?" },
                //    { "/company/update", "Wirklich speichern?" }
                //};

                //DataTable dtCompany = Program.Sql.GetAllCompanys();
                //builder.Append(MelBoxWeb.HtmlFormCompany(dtCompany, companyId));
                //builder.Append(MelBoxWeb.HtmlEditor(action));
            }

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/update")]
        public IHttpContext UpdateMelBoxCompany(IHttpContext context)
        {
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
           // Console.WriteLine("\r\n company/update payload:\r\n" + payload);

            string[] args = payload.Split('&');
            int companyId = 0;
            string name = string.Empty;
            string address = string.Empty;
            string city = string.Empty;

            foreach (string arg in args)
            {
                if (arg.StartsWith("Id="))
                {
                    companyId = int.Parse(arg.Split('=')[1]);
                }

                if (arg.StartsWith("Name="))
                {
                    name = arg.Split('=')[1].Replace('+', ' ');
                }

                if (arg.StartsWith("Adresse="))
                {
                    address = arg.Split('=')[1].Replace('+', ' ');
                }

                if (arg.StartsWith("Ort="))
                {
                    city = arg.Split('=')[1].Replace('+', ' ');
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Änderung Firmendaten"));

            if (0 == Program.Sql.UpdateCompany(companyId, name, address, city))
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Keine &Auml;nderungen für Firma '" + name + "'", "Es wurden keine Änderungen übergeben oder der Aufruf war fehlerhaft."));
            }
            else
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Änderungen für Firma '" + name + "' gespeichert", "Die Änderungen an Firma '" + name + "' wurden in der Datenbank gespeichert."));
            }

            if (companyId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Aufruffehler", "Der Aufruf dieser Seite war fehlerhaft."));                
            }
            else
            {
                builder.Append(MelBoxWeb.HtmlUnitCompany(companyId));
            }

            //Dictionary<string, string> action = new Dictionary<string, string>
            //    {
            //        { "/company/create", "Firma neu anlegen?" },
            //        { "/company/update", "Wirklich speichern?" }
            //    };
            
            //#region Firma anzeigen
            //DataTable dtCompany = Program.Sql.GetAllCompanys();
            //builder.Append(MelBoxWeb.HtmlFormCompany(dtCompany, companyId));
            //builder.Append(MelBoxWeb.HtmlEditor(action));
            //#endregion

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/company/create")]
        public IHttpContext AddMelBoxCompany(IHttpContext context)
        {
            string payload = context.Request.Payload;
            payload = MelBoxWeb.DecodeUmlaute(payload);
            Console.WriteLine("\r\n company/create payload:\r\n" + payload);

            string[] args = payload.Split('&');
            string name = string.Empty;
            string address = string.Empty;
            string city = string.Empty;

            foreach (string arg in args)
            {
                if (arg.StartsWith("Name="))
                {
                    name = arg.Split('=')[1].Replace('+', ' ');
                }

                if (arg.StartsWith("Adresse="))
                {
                    address = arg.Split('=')[1].Replace('+', ' ');
                }

                if (arg.StartsWith("Ort="))
                {
                    city = arg.Split('=')[1].Replace('+', ' ');
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(MelBoxWeb.HtmlHead("Änderung Firmendaten"));

            if ( name.Length < 3)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Kein neuer Firmeneintrag", "Der Firmenname muss mindestens 3 Zeichen lang sein.") );
            }
            else if (!Program.Sql.InsertCompany(name, address, city) )
            {
                builder.Append(MelBoxWeb.HtmlAlert(1, "Fehler neuer Firmeneintrag", "'" + name + "', '" + address + "', '" + city + "'<br>" +
                    "Es konnte kein neuer Eintrag in die Datenbank geschrieben werden."));
            } 
            else
            {
                builder.Append(MelBoxWeb.HtmlAlert(3, "Neuer Firmeneintrag", "Die Firma <p>'" + name + "'<br>'" + address + "'<br>'" + city + "'</p>" +
                        "wurde erfolgreich in die Datenbank aufgenommen."));
            }

            Dictionary<string, string> action = new Dictionary<string, string>
                {
                    { "/company/create", "Firma neu anlegen?" },
                    { "/company/update", "Wirklich speichern?" }
                };

            #region Firma anzeigen
            DataTable dtCompany = Program.Sql.GetAllCompanys();

            int lastId = int.MinValue;
            foreach (DataRow dr in dtCompany.Rows)
            {
                int id = dr.Field<int>("Id");
                lastId = Math.Max(lastId, id);
            }

            builder.Append(MelBoxWeb.HtmlUnitCompany(lastId));

            builder.Append(MelBoxWeb.HtmlFoot());
            #endregion

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }

        #endregion

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
                    Program.Sql.Log(MelBoxSql.LogTopic.Sql, MelBoxSql.LogPrio.Info, "Benutzer " + myId + " '" + name + "' ist angemeledt.");

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

            builder.Append("<p>RawUrl: " + context.Request.RawUrl + "</p>");
#endif
            #endregion

            builder.Append(MelBoxWeb.HtmlFoot());

            context.Response.SendResponse(MelBoxWeb.EncodeUmlaute(builder.ToString()));
            return context;
        }
    }

}

//////////////////////////////////////////////////////////////////////////////////////////////
/*** MAGAZIN
        //        [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/login")]
        //        public IHttpContext LogIn(IHttpContext context)
        //        {
        //            string name = string.Empty;
        //            string password = string.Empty;
        //            string payload = context.Request.Payload;
        //            string[] args = payload.Split('&');

        //            foreach (string arg in args)
        //            {
        //                if (arg.StartsWith("name="))
        //                {
        //                    name = arg.Split('=')[1];
        //                }

        //                if (arg.StartsWith("password="))
        //                {
        //                    password = arg.Split('=')[1];
        //                }
        //            }

        //            LogedInContactId = 0;

        //            StringBuilder builder = new StringBuilder();

        //#if DEBUG
        //            LogedInContactId = 1;
        //#else

        //            if (name.Length > 3 && password.Length > 3)
        //            {
        //                if (password == MelBoxWebServer.MasterPassword)
        //                {
        //                    LogedInContactId = 1;
        //                }
        //                else
        //                {
        //                    LogedInContactId = Program.Sql.GetContactIdFromLogin(name, password);
        //                }
        //            }
        //#endif
        //            //   builder.Append(MelBoxWeb.HtmlHead("Log-In", LogedInContactId));
        //            builder.Append("<div class='w3-panel " + (LogedInContactId != 0 ? "w3-pale-green" : "w3-yellow") + " w3-border'>\n");
        //            builder.Append(" <h2>LogIn " + (LogedInContactId != 0 ? "erfolgreich" : "fehlgeschlagen") + "</h2>");
        //            builder.Append(" <p></p>");
        //            //builder.Append("<script>\n"); 
        //            //builder.Append("  if (typeof(Storage) !== \"undefined\") {");
        //            //builder.Append("    localStorage.LogIn = \"" + LogedInContactId + "\"");

        //            ////document.getElementById("result").innerHTML = localStorage.lastname
        //            //builder.Append("</script>\n");
        //            builder.Append("</div>\n");
        //            builder.Append(MelBoxWeb.HtmlFoot());
        //            context.Response.SendResponse(builder.ToString());
        //            return context;

        //            //http://localhost:1234/repeat?word=parrot
        //            //var password = context.GetPropertyValueAs<string>("password");

        //            //var password = context.Request.QueryString["password"] ?? "what?";
        //        }

        [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/repeat")]
        public IHttpContext RepeatMe(IHttpContext context)
        {
            //http://localhost:1234/repeat?word=parrot
            var word = context.Request.QueryString["word"] ?? "what?";

            context.Response.SendResponse(word);
            return context;
        }
//*/