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
        #region Werkzeuge
        public static string EncodeUmlaute(string input)
        {
            return input.Replace("Ä", "&Auml;").Replace("Ö", "&Ouml;").Replace("Ü", "&Uuml;").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;").Replace("ß", "&szlig;");
        }

        public static string DecodeUmlaute(string input)
        {
            return input.Replace("&Auml;", "Ä").Replace("&Ouml;", "Ö").Replace("&Uuml;", "Ü").Replace("&auml;", "ä").Replace("&ouml;", "ö").Replace("&uuml;", "ü").Replace("&szlig;", "ß").Replace("%40", "@");
        }

        public static string GenerateID(int contactId)
        {
            string guid = Guid.NewGuid().ToString("N");

            while (MelBoxWeb.LogedInGuids.Count > 20) //nicht mehr als 20 Ids merken
            {
                MelBoxWeb.LogedInGuids.Remove(MelBoxWeb.LogedInGuids.FirstOrDefault().Key);
            }

            MelBoxWeb.LogedInGuids.Add(guid, contactId);
            return guid;
        }
      
        public static Dictionary<string, string> ReadPayload(string payload)
        {
            payload = payload ?? string.Empty;            
            Dictionary<string, string> dict = new Dictionary<string, string>();

            payload = MelBoxWeb.DecodeUmlaute(payload);

            string[] args = payload.Split('&');

            foreach (string arg in args)
            {
                string[] items = arg.Split('=');

                if (items.Length > 1)
                dict.Add(items[0], items[1]);              
            }

            return dict;
        }

        #endregion

        #region Bausteine

        public static string HtmlHead(string htmlTitle, string guid = "")
        {
            if (guid == null) guid = string.Empty;

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
            builder.Append("<script src='https://www.w3schools.com/lib/w3.js'></script>");
            builder.Append("<script>\n");
            builder.Append("function readguid() {\n");
            // builder.Append(" alert('This message was triggered from the onload event');\n");

            if (guid.Length > 0)
            {
                builder.Append(" sessionStorage.setItem('guid','" + guid + "');\n ");                
            }

            builder.Append(" if ( sessionStorage.getItem('guid') !== null) {\n");
            builder.Append("  document.getElementById('guid').value = sessionStorage.getItem('guid');");
            builder.Append("  w3.removeClass('.w3-disabled','w3-disabled'); \n");            
            builder.Append(" }\n");
            builder.Append("}\n");
            builder.Append("</script>\n");

            builder.Append("</head>\n");
            builder.Append("<body onload='readguid()'>\n");
            builder.Append("<div class='w3-display-topright w3-opacity'>" + DateTime.Now + "</div>\n");

            builder.Append(HtmlMainMenu());

            builder.Append("<center>\n");
            builder.Append("<div class='w3-container w3-cyan w3-margin-bottom'>\n");
            builder.Append("<h1>MelBox2 - " + htmlTitle + "</h1>\n</div>\n\n");

            return builder.ToString();
        }
      
        private static string HtmlMainMenu()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-bar w3-border'>\n");
            builder.Append("<a href='/' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>login</i></a>");

            builder.Append("<a href='/in' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>drafts</i></a>");
            builder.Append("<a href='/out' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>redo</i></a>");
            builder.Append("<a href='/overdue' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>alarm</i></a>");


            builder.Append("<a href='/blocked' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></a>");

            builder.Append("<a href='/log' class='w3-button class='w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>assignment</i></a>");

            //builder.Append("<a href='\\" + guid + "\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>login</i></a>");
            //builder.Append("<a href='\\" + guid + "\\log' class='w3-bar-item w3-button " + disabled + "'><i class='w3-xxlarge material-icons-outlined'>assignment</i></a>");

            //builder.Append("<a href='.\\' class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>login</i></a>\n");
            //builder.Append("<button onclick=\"document.location='/in'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>drafts</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/out'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>redo</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/overdue'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>alarm</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/blocked'\" class='w3-bar-item w3-button' ><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/shift'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>event</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/account'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/log'\" class='w3-bar-item w3-button' " + disabled + "><i class='w3-xxlarge material-icons-outlined'>assignment</i></button>\n");
            //builder.Append("<button onclick=\"document.location='/'\" class='w3-bar-item w3-button'><i class='w3-xxlarge material-icons-outlined'>person</i></button>\n");

            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlFoot()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("</center>\n");
            builder.Append("<div class='w3-container w3-cyan w3-margin-top w3-right-align '>\n");
            builder.Append(" <input id='guid' name='guid' form='form1' class='w3-cyan w3-text-blue w3-tiny' readonly>\n"); //w3-opacity
            builder.Append("</div>\n</body>\n</html>");
            return builder.ToString();
        }

        public static string HtmlLogIn()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-row w3-margin'>\n");
            builder.Append(" <div class='w3-container w3-quarter'></div>\n");
            builder.Append(" <div class='w3-card w3-half'>\n");
            builder.Append("  <div class='w3-container w3-cyan'>\n");
            builder.Append("    <h2>LogIn</h2>\n");
            builder.Append("  </div>\n");
            builder.Append("  <form class='w3-container' action='/' method='post'>\n");
            builder.Append("    <input style='display:none;' name='p' value='login'>\n");
            builder.Append("    <label class='w3-text-grey'><b>Benutzer</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='name' type='text' placeholder='Benutzername (von anderen sichtbar)' required></p>\n");
            builder.Append("    <label class='w3-text-grey'><b>Passwort</b></label>\n");
            builder.Append("    <input class='w3-input w3-border w3-sand' name='password' type='password' pattern='(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{ 6,}' placeholder='Mind. 6 Zeichen; Gro&szlig;- und Kleinbuchstaben, Zahl' required></p>\n");
            builder.Append("    <p>\n");
            builder.Append("    <button class='w3-button w3-teal'>LogIn</button>\n</p>\n");
            builder.Append("  </form>\n");
            builder.Append(" </div>\n</div>\n");

            return builder.ToString();
        }

        public static string HtmlAlert(int prio, string caption, string alarmText)
        {
            StringBuilder builder = new StringBuilder();

            switch (prio)
            {
                case 1:
                    builder.Append("<div class='w3-panel w3-pale-red'>\n");
                    break;
                case 2:
                    builder.Append("<div class='w3-panel w3-pale-yellow'>\n");
                    break;
                case 3:
                    builder.Append("<div class='w3-panel w3-pale-green'>\n");
                    break;
                default:
                    builder.Append("<div class='w3-panel w3-pale-blue'>\n");
                    break;
            }

            builder.Append(" <h3>" + caption + "</h3>\n");
            builder.Append(" <p>" + alarmText + "</p>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlEditor(string actionPath, string buttonText ="editieren")
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div id='Editor' class='w3-modal'>\n"); //
            builder.Append("  <div class='w3-modal-content w3-card-4' style='max-width:600px'>\n");
            builder.Append("  <div class='w3-container'>\n");
            builder.Append("   <span onclick=\"w3.hide('#Editor')\" class='w3-button w3-display-topright'>&times;</span>");
            builder.Append("   <form id='form1' method='post' class='w3-margin' action='" + actionPath + "'>\n");
            builder.Append("    <button class='w3-button w3-block w3-teal w3-section w3-padding-large w3-margin' type='submit'>" + MelBoxWeb.EncodeUmlaute(buttonText) + "</button>");

            builder.Append("   </form>\n");
            builder.Append("  </div>\n");
            builder.Append("  </div>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        #endregion

        #region Tabellen
        //public static string HtmlTablePlain(DataTable dt)
        //{
        //    //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

        //    StringBuilder builder = new StringBuilder();

        //    builder.Append("<div class='w3-container'>\n");
        //    builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
        //    builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
        //    builder.Append("</div>");

        //    builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
        //    builder.Append("<tr class='w3-teak'>\n\t");

        //    foreach (DataColumn c in dt.Columns)
        //    {
        //        builder.Append("<th>");
        //        builder.Append(c.ColumnName);
        //        builder.Append("</th>");
        //    }
        //    builder.Append("\n</tr>\n");
        //    foreach (DataRow r in dt.Rows)
        //    {
        //        builder.Append("<tr class='myRow'>\n\t");
        //        foreach (DataColumn c in dt.Columns)
        //        {
        //            builder.Append("<td>");
        //            builder.Append(r[c.ColumnName]);
        //            builder.Append("</td>");
        //        }
        //        builder.Append("\n</tr>\n");
        //    }
        //    builder.Append("</table>\n");

        //    return builder.ToString();
        //}

        public static string HtmlTablePlain(DataTable dt, bool canUserEdit = false)
        {
            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-container'>\n");
            builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
            builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
            builder.Append("</div>");

            builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
            builder.Append("<tr class='w3-teak'>\n\t");

            if (canUserEdit)
            {
                builder.Append("<th>Edit</th>");
            }

            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }

            builder.Append("\n</tr>\n");

            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr class='myRow'>\n\t");

                if (canUserEdit)
                {
                    builder.Append("<td><input class='w3-radio' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>");
                }

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


        public static string HtmlTableShift(DataTable dt)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<table class='w3-table-all w3-hoverable w3-cell'>\n");
            builder.Append("<tr class='w3-teak'>\n");
            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                switch (c.ColumnName)
                {
                    case "SendSms":
                        builder.Append("<i class='w3-xxlarge material-icons-outlined'>smartphone</i>");
                        break;
                    case "SendEmail":
                        builder.Append("<i class='w3-xxlarge material-icons-outlined'>email</i>");
                        break;
                    default:
                        builder.Append(c.ColumnName);
                        break;
                }

                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");

            int procent;

            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr>\n\t");
                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");
                    switch (c.ColumnName)
                    {

                        case "SendSms":
                            builder.Append("<input class='w3-check w3-center' type='checkbox' name='SendSms' ");
                            builder.Append((int.Parse(r[c.ColumnName].ToString()) > 0) ? "checked" : string.Empty);
                            builder.Append(" disabled>");
                            break;
                        case "SendEmail":
                            builder.Append("<input class='w3-check w3-center' type='checkbox' name='SendEmail' ");
                            builder.Append((int.Parse(r[c.ColumnName].ToString()) > 0) ? "checked" : string.Empty);
                            builder.Append(" disabled>");
                            break;
                        case "Beginn":
                            procent = int.Parse(r[c.ColumnName].ToString()) * 100 / 24;
                            builder.Append("<div class='w3-teal' style='width:240px;'>\n  <div class='w3-grey' style='width:" + procent + "%'>" + r[c.ColumnName] + "&nbsp;Uhr</div>\n</div>\n");
                            break;
                        case "Ende":
                            procent = int.Parse(r[c.ColumnName].ToString()) * 100 / 24;
                            builder.Append("<div class='w3-grey' style='width:240px;'>\n <div class='w3-teal' style='width:" + procent + "%'>" + r[c.ColumnName] + "&nbsp;Uhr</div>\n</div>\n");
                            break;
                        default:
                            builder.Append(r[c.ColumnName]);
                            break;
                    }
                    builder.Append("</td>\n");
                }
                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>\n");

            return builder.ToString();
        }

        public static string HtmlTableBlocked(DataTable dt, bool canUserEdit = false)
        {
            //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-container'>\n");
            builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
            builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
            builder.Append("</div>");

            builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
            builder.Append("<tr class='w3-teak'>\n\t");

            if (canUserEdit)
            {
                builder.Append("<th>FRG</th>");
            }

            foreach (DataColumn c in dt.Columns)
            {
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>");
            }
            builder.Append("\n</tr>\n");

            foreach (DataRow r in dt.Rows)
            {
                builder.Append("<tr class='myRow'>\n\t");

                if (canUserEdit)
                {
                    builder.Append("<td><input class='w3-radio' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>");
                }

                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");
                    switch (c.ColumnName)
                    {

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

                            builder.Append("<input class='w3-check' form='form1' name='" + c.ColumnName + "' type='checkbox' " + check + " disabled>\n");
                            break;
                        case "Beginn":
                        case "Ende":
                                builder.Append("<select class='w3-select' form='form1' name='" + c.ColumnName + "' " + (canUserEdit ? string.Empty : "disabled") + ">\n");

                                int current = int.Parse(r[c.ColumnName].ToString().Substring(0, 2));
                                for (int i = 0; i < 24; i++)
                                {
                                    builder.Append("<option value='" + i + "' " + ((current == i) ? "selected" : string.Empty) + ">" + i +" Uhr</option>\n");
                                }

                                builder.Append("</select>\n");
                                break;
                        default:
                            builder.Append(r[c.ColumnName]);
                            break;
                    }
                    builder.Append("</td>");
                }
                builder.Append("\n</tr>\n");
            }
            builder.Append("</table>\n");

            return builder.ToString();
        }

        #endregion


    }
}
