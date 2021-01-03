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
            return input.Replace("%C4", "Ä").Replace("%D6", "Ö").Replace("%DC", "Ü").Replace("%E4", "ä").Replace("%F6", "ö").Replace("%FC", "ü")
                .Replace("%DF", "ß").Replace("%40", "@").Replace("%2B", "+");
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
            builder.Append("  document.getElementById('buttonaccount').href = '/account/' + sessionStorage.getItem('guid');");
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
            builder.Append("<a href='/' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>login</i></a>");
            builder.Append("<span class='w3-bar-item' style='width:50px;'></span>");

            builder.Append("<a href='/in' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>drafts</i></a>");
            builder.Append("<a href='/out' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>forward_to_inbox</i></a>");
            builder.Append("<a href='/overdue' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>alarm</i></a>");
            builder.Append("<span class='w3-bar-item' style='width:50px;'></span>");

            builder.Append("<a href='/blocked' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>alarm_off</i></a>");
            builder.Append("<a href='/account' id='buttonaccount' class='w3-button w3-bar-item w3-disabled'><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></a>");

            builder.Append("<span class='w3-bar-item' style='width:50px;'></span>");
            builder.Append("<a href='/log' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>assignment</i></a>");

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

        #region Formulare
        public static string HtmlEditor(string actionPath, string buttonText = "editieren")
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

        public static string HtmlFormAccount(DataTable dtAccount, DataTable dtCompany)
        {
            StringBuilder builder = new StringBuilder();
                      
            builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
            builder.Append("<h2 class='w3-center'>Benutzerkonto</h2>\n");

            int companyId = 0;
            foreach (DataRow r in dtAccount.Rows)
            {
                foreach (DataColumn c in dtAccount.Columns)
                {
                    switch (c.ColumnName)
                    {
                        //ContactId,  Name, Passwort, CompanyId, Firma, Email, Telefon, SendSms, SendEmail, Max_Inaktivität 

                        case "ContactId":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>flag</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-grey w3-disabled' type='text' placeholder='Laufende Nummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' readonly>\n");    
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Name":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>person</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Passwort":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>vpn_key</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='password'  placeholder='Passwort' name='" + c.ColumnName + "' id='" + c.ColumnName + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "CompanyId":
                            companyId = int.Parse( r[c.ColumnName].ToString() );
                            break;

                        case "Firma":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>work</i></div>\n");

                            builder.Append("<script>\n");
                            builder.Append("function myFunction() {\n");
                            //builder.Append(" alert('Setze Wert ');\n");
                            builder.Append(" document.getElementById('buttonCompanySettings').href = '/company/' + document.getElementById('" + c.ColumnName + "' ).value;\n");
                            builder.Append("}\n</script>\n");
                            builder.Append(" <a href='/company/" + companyId + "' id='buttonCompanySettings' style='max-width:60px' class='w3-col w3-button'><i class='w3-xlarge material-icons-outlined'>settings</i></a>");                           
                       
                            builder.Append(" <div class='w3-rest'>\n");                        
                            builder.Append("  <select form='form1' class='w3-select w3-border w3-disabled' name='CompanyId' id='" + c.ColumnName + "'  onchange='myFunction()'>\n");

                            foreach (DataRow row in dtCompany.Rows)
                            {           
                                builder.Append("   <option value='" + row["Id"] + "' ");
                                builder.Append( ( companyId.ToString() == row["Id"].ToString() ) ? "selected" : string.Empty);
                                builder.Append(">" + row["Name"].ToString() + "</option>\n");
                            }

                            builder.Append("  </select>\n");
                            builder.Append(" </div>\n</div>\n");
                            break;


                        case "Telefon":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>phone</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='tel'  placeholder='Mobiltelefonnummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='+" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Email":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='email' pattern='[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$' placeholder='Emailadresse' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "SendSms":                        
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_phone</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-check w3-border w3-margin w3-disabled' type='checkbox' placeholder='SMS empfangen' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "SendEmail":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_mail</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-check w3-border w3-margin w3-disabled' type='checkbox' placeholder='Email empfangen' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Max_Inaktivität":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>more_time</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='number' min='0' step='8' placeholder='Maximale Inaktivität in Stunden' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        default:
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>" + c.ColumnName + "</div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;
                    }

                }
            }

            builder.Append(" <input type='reset' form='form1' class='w3-button w3-block w3-section w3-pale-blue w3-ripple w3-padding w3-half'>\n");
            builder.Append(" <button class='w3-button w3-block w3-section w3-teal w3-ripple w3-padding w3-half' onclick =\"document.getElementById('Editor').style.display = 'block'\">Speichern</button>\n");
            builder.Append("</div>\n<center>\n");

            return builder.ToString();
        }
               
        public static string HtmlFormCompany(DataTable dtCompany, int companyId)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
            builder.Append("<h2 class='w3-center'>Firmenkonto</h2>\n");

            #region Firmenauswahl
            builder.Append(" <div class='w3-pale-blue'>\n");
            builder.Append("<script>\n");
            builder.Append("function myFunction() {\n");
            builder.Append(" document.getElementById('buttonCompanySettings').href = '/company/' + document.getElementById('selectedCompany').value;\n");
            builder.Append("}\n</script>\n");
            builder.Append(" <a href='/company/" + companyId + "' id='buttonCompanySettings' style='max-width:60px' class='w3-col w3-button'><i class='w3-xlarge material-icons-outlined'>settings</i></a>");
            builder.Append("  <select class='w3-select w3-border w3-disabled' id='selectedCompany'  onchange='myFunction()'>\n");

            foreach (DataRow row in dtCompany.Rows)
            {
                builder.Append("   <option value='" + row["Id"] + "' ");
                builder.Append((companyId.ToString() == row["Id"].ToString()) ? "selected" : string.Empty);
                builder.Append(">" + row["Name"].ToString() + "</option>\n");
            }

            builder.Append("  </select>\n");
            builder.Append(" </div>\n");
            #endregion

            foreach (DataRow r in dtCompany.Rows)
            {
                if (companyId.ToString() != r["Id"].ToString()) continue;

                foreach (DataColumn c in dtCompany.Columns)
                {
                    string icon;
                    string placeholder;
                    string inputReadonly = string.Empty;

                    switch (c.ColumnName)
                    {
                        case "Id":
                            icon = "flag";
                            placeholder = "Eindeutige Nummer";
                            inputReadonly = "readonly";
                            break;

                        case "Name":
                            icon = "local_offer";
                            placeholder = "Firmenname";
                            break;

                        case "Address":
                            icon = "location_on";
                            placeholder = "Adresse oder Werk";
                            break;

                        case "City":
                            icon = "location_city";
                            placeholder = "Ort";
                            break;

                        default:
                            icon = "info";
                            placeholder = c.ColumnName;
                            break;
                    }


                    builder.Append("<div class='w3-row w3-section'>\n");
                    builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>" + icon + "/i></div>\n");
                    builder.Append(" <div class='w3-rest'>\n");
                    builder.Append("  <input form='form1' class='w3-input w3-border w3-grey w3-disabled' type='text' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' placeholder='" + placeholder + "' " + inputReadonly + ">\n");
                    builder.Append(" </div>\n</div>\n");
                }
                break;
            }

            builder.Append(" <input type='reset' form='form1' class='w3-button w3-block w3-section w3-pale-blue w3-ripple w3-padding w3-half'>\n");
            builder.Append(" <button class='w3-button w3-block w3-section w3-teal w3-ripple w3-padding w3-half' onclick =\"document.getElementById('Editor').style.display = 'block'\">Speichern</button>\n");
            builder.Append("</div>\n<center>\n");

            return builder.ToString();
        }
        #endregion

    }
}
