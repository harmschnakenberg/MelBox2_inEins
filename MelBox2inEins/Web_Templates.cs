﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{

    public static class MyStyle
    {
        public static string Background { get; set; } = "w3-light-grey";

        public static string Panel { get; set; } = "w3-cyan";
      
        public static string PanelLight { get; set; } = "w3-pale-blue";

        public static string Button { get; set; } = "w3-teal";
    }
    public partial class MelBoxWeb
    {

        public static string HtmlCanvas(string htmlContent, string pageCaption = "Meldeprogramm", string logedInUserName = "", string newGuid = "")
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(HtmlHead(logedInUserName, newGuid));
            builder.Append(HtmlMainMenu());
            builder.Append(HtmlPageCaption(pageCaption));

            builder.Append(htmlContent);

            builder.Append(HtmlFoot());
            return EncodeUmlaute(builder.ToString());
        }

        private static string HtmlHead(string logedInUserName, string newGuid = "")
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<html class='" + MyStyle.Background + "'>\n");
            builder.Append("<head>\n");

            builder.Append("<link rel='shortcut icon' href='https://www.kreutztraeger-kaeltetechnik.de/wp-content/uploads/2016/12/favicon.ico'>\n");
            builder.Append("<title>");
            builder.Append("MelBox2");
            builder.Append("</title>\n");
            builder.Append("<meta name='viewport' content='width=device-width, initial-scale=1'>\n");
            builder.Append("<link rel='stylesheet' href='https://www.w3schools.com/w3css/4/w3.css'>\n");
            builder.Append("<link rel='stylesheet' href='https://fonts.googleapis.com/icon?family=Material+Icons+Outlined'>\n");

            #region Globale JavaScripts
            builder.Append("<script src='https://www.w3schools.com/lib/w3.js'></script>\n");
            builder.Append("<script>\n");
            builder.Append("function readguid() {\n");

            if (newGuid.Length > 0)
            {
                builder.Append(" sessionStorage.setItem('guid','" + newGuid + "');\n ");
            }

            builder.Append(" if ( sessionStorage.getItem('guid') !== null) {\n");
            builder.Append("  var guid = sessionStorage.getItem('guid');\n");
            builder.Append("  document.getElementById('guid').value = guid;\n");
            builder.Append("  document.getElementById('buttonAccount').disabled = false;\n");
            builder.Append(" }\n");
            builder.Append("}\n");
            builder.Append("</script>\n");
            #endregion

            builder.Append("</head>");

            builder.Append("<body onload='readguid()'>\n");
            builder.Append(" <div class='w3-display-topright w3-opacity'>" + logedInUserName + " " + DateTime.Now + "</div>\n");
            return builder.ToString();
        }

        private static string HtmlFoot()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(" <p class='w3-container " + MyStyle.Panel + " w3-margin-top w3-right-align '>");
            builder.Append("  <a href='#top' class='w3-button " + MyStyle.Button + "'><i class='w3-xlarge material-icons-outlined'>north</i></a>");
            builder.Append(" </p>\n");
            builder.Append(" <input type='hidden' id='guid' name='guid' form='form1' class='w3-cyan w3-text-light-blue w3-tiny w3-border-0' readonly>\n");
            builder.Append("</body>\n");
            builder.Append("</html>\n");

            return builder.ToString();
        }

        private static string HtmlPageCaption(string caption)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<center>\n");
            builder.Append(" <div class='w3-container " + MyStyle.Panel + " w3-margin-bottom'>\n");
            builder.Append("  <h1>MelBox2 - " + caption + "</h1>\n");
            builder.Append("  <input form='form1' type='hidden' name='pageTitle' value='" + caption + "'>\n");
            builder.Append(" </div>\n");
            builder.Append("</center>\n\n");

            return builder.ToString();
        }

        private static string HtmlMainMenu()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-bar " + MyStyle.Background + "'>\n");
            builder.Append(" <form id='form1' method='post' >\n");

            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/'><i class='w3-xxlarge material-icons-outlined'>login</i></button>\n");
            builder.Append("  <div class='w3-bar-item'>&nbsp;</div>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/in'><i class='w3-xxlarge material-icons-outlined'>drafts</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/out'><i class='w3-xxlarge material-icons-outlined'>forward_to_inbox</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/overdue'><i class='w3-xxlarge material-icons-outlined'>pending_actions</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/blocked'><i class='w3-xxlarge material-icons-outlined'>notifications_off</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/shift'><i class='w3-xxlarge material-icons-outlined'>event_note</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/account' id='buttonAccount' disabled><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></button>\n");
            builder.Append("  <button class='w3-bar-item w3-button' type='submit' formaction='/log'><i class='w3-xxlarge material-icons-outlined'>assignment</i></button>\n");


            builder.Append(" </form>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlAlert(int prio, string caption, string alarmText)
        {
            StringBuilder builder = new StringBuilder();

            switch (prio)
            {
                case 1:
                    builder.Append("<div class='w3-panel w3-margin-left w3-pale-red w3-leftbar w3-border-red'>\n");
                    break;
                case 2:
                    builder.Append("<div class='w3-panel w3-margin-left w3-pale-yellow w3-leftbar w3-border-yellow'>\n");
                    break;
                case 3:
                    builder.Append("<div class='w3-panel w3-margin-left w3-pale-green w3-leftbar w3-border-green'>\n");
                    break;
                default:
                    builder.Append("<div class='w3-panel w3-margin-left w3-pale-blue w3-leftbar w3-border-blue'>\n");
                    break;
            }

            builder.Append(" <h3>" + caption + "</h3>\n");
            builder.Append(" <p>" + alarmText + "</p>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlLogin()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-row w3-margin'>\n");
            builder.Append(" <div class='w3-container w3-quarter'></div>\n");
            builder.Append(" <div class='w3-card w3-half'>\n");
            builder.Append("  <div class='w3-container " + MyStyle.Panel + "'>\n");
            builder.Append("    <h2>Login</h2>\n");
            builder.Append("  </div>\n");
            builder.Append("  <div class='w3-margin'>\n");
            builder.Append("    <label><b>Benutzer</b></label>\n");
            builder.Append("    <input form='form1' class='w3-input w3-border w3-sand' name='name' type='text' placeholder='Benutzername (von anderen sichtbar)' required></p>\n");
            builder.Append("    <label><b>Passwort</b></label>\n");
            //builder.Append("    <input class='w3-input w3-border w3-sand' name='password' type='password' pattern='(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{6,}' placeholder='Mind. 6 Zeichen; Gro&szlig;- und Kleinbuchstaben, Zahl' required></p>\n");
            builder.Append("    <input form='form1' class='w3-input w3-border w3-sand' name='password' type='password' pattern='.{4,}' placeholder='Mind. 6 Zeichen; Gro&szlig;- und Kleinbuchstaben, Zahl' required></p>\n");
            builder.Append("    <p>\n");
            builder.Append("    <button form='form1' class='w3-button " + MyStyle.Button + "' formaction='/'>Login</button>\n</p>\n");
            builder.Append("  </div>\n");

            builder.Append(" </div>\n</div>\n");

            return builder.ToString();
        }

        public static string HtmlLogout()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("    <button class='w3-button " + MyStyle.Button + "' onclick='deleteguid()'>Logout</button>\n</p>\n");

            builder.Append("<script>\n");
            builder.Append("function deleteguid() {\n");
            //builder.Append(" alert('This message was triggered from the onclick event');\n");
            builder.Append(" document.getElementById('guid').value = '';\n");
            builder.Append(" sessionStorage.removeItem('guid');\n ");
            builder.Append(" window.open('/', '_self'); \n");
            builder.Append("}\n");
            builder.Append("</script>\n");

            return builder.ToString();
        }

        public static string HtmlEditor(Dictionary<string, string> action)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<center>\n");
            builder.Append("<div id='Editor' class='w3-modal'>\n");
            builder.Append("  <div class='w3-quarter'>&nbsp;</div>\n");
            builder.Append("  <div class='w3-modal-content w3-card-4 w3-half'>\n");
            builder.Append("  <div class='w3-container'>\n");
            builder.Append("  <div class='w3-margin'><p>Optionen</p></div>\n");
            builder.Append("   <span onclick=\"w3.hide('#Editor')\" class='w3-button w3-margin-bottom w3-display-topright'><i class='w3-xxlarge material-icons-outlined'>close</i></span>");
          
            foreach (var path in action.Keys)
            {
                builder.Append("    <button class='" + MyStyle.Button + " w3-button w3-padding-large w3-margin' ");
                builder.Append("form='form1' formaction='" + path + "' type='submit'>" + action[path] + "</button>");
            }

            builder.Append("  </div>\n");
            builder.Append("  </div>\n");
            builder.Append("</div>\n");
            builder.Append("</center>\n");

            return builder.ToString();
        }

        private static string HtmlSearchbar()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-container'>\n");
            builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
            builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin-bottom w3-border w3-round-large' placeholder='Suche nach..'>\n");
            builder.Append("</div>");

            return builder.ToString();
        }

        public static string HtmlInfoSidebar(string title, string infoText)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("<button class='w3-button w3-display-bottomleft' onclick='w3_open()'><i class='w3-xxlarge material-icons-outlined'>info</i></button>\n");

            builder.Append("<div class='w3-card-4 w3-display-middle w3-sand w3-threequarter w3-bar w3-border' style='display:none' id='infoSidebar'>\n");
            builder.Append(" <button onclick='w3_close()' class='w3-button w3-right w3-bar-item w3-large'><i class='w3-xxlarge material-icons-outlined'>close</i></button>\n");
            
            builder.Append(" <div class='w3-panel'>\n");
            builder.Append(" <i class='w3-jumbo material-icons-outlined'>info</i>\n");
            builder.Append("  <h3>" + title + "</h3>\n");
            builder.Append("  <div>" + infoText + "</div>");
            builder.Append(" </div>\n");
            builder.Append("</div>");
           
            builder.Append("<script>\n");
            builder.Append(" function w3_open() {\n");
            builder.Append("  document.getElementById('infoSidebar').style.display = 'block';\n");
            builder.Append(" }\n");
            builder.Append(" function w3_close() {\n");
            builder.Append("  document.getElementById('infoSidebar').style.display = 'none';\n");
            builder.Append(" }\n");
            builder.Append("</script>\n");

            return builder.ToString();
        }


        //public static string HtmlTablePlain(DataTable dt)
        //{
        //    StringBuilder builder = new StringBuilder();

        //    builder.Append(HtmlSearchbar());

        //    builder.Append("<center class='w3-responsive'>\n");
        //    builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");

        //    //Überschriften
        //    builder.Append(" <tr class='" + MyStyle.PanelLight + "'>\n\t");

        //    foreach (DataColumn c in dt.Columns)
        //    {
        //        builder.Append("<th>");
        //        builder.Append(c.ColumnName);
        //        builder.Append("</th>\n");
        //    }

        //    builder.Append("\n </tr>\n");

        //    //Inhalt
        //    foreach (DataRow r in dt.Rows)
        //    {
        //        builder.Append(" <tr class='myRow'>\n\t");

        //        foreach (DataColumn c in dt.Columns)
        //        {
        //            builder.Append("<td>");
        //            builder.Append(r[c.ColumnName]);
        //            builder.Append("</td>");
        //        }

        //        builder.Append("\n </tr>\n");
        //    }

        //    builder.Append("</table>\n");
        //    builder.Append("</center>\n");

        //    return builder.ToString();
        //}

        public static string HtmlTablePlain(DataTable dt, bool isAdmin = false)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlSearchbar());

            builder.Append("<center class='w3-responsive'>\n");
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");

            #region Überschriften
            builder.Append(" <tr class='" + MyStyle.PanelLight + "'>\n\t");

            if (isAdmin)
            {
                builder.Append("<th><i class='w3-xxlarge material-icons-outlined'>create</i></th>");
            }

            foreach (DataColumn c in dt.Columns)
            {
                // builder.Append("<th onclick=\"sortTable(document.getElementById('myTable'), " + c.Ordinal + ")\">"); //
                builder.Append("<th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>\n");
            }

            builder.Append("\n </tr>\n");
            #endregion

            #region Inhalt
            foreach (DataRow r in dt.Rows)
            {
                builder.Append(" <tr class='myRow'>\n\t");

                if (isAdmin)
                {
                    builder.Append("<td><input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>");
                }

                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");

                    switch (c.ColumnName)
                    {
                        case "Empfangen":
                        case "Gesendet":
                        case "Letzte_Nachricht":                            
                            builder.Append(DateTime.Parse(r[c.ColumnName].ToString()).ToLocalTime());
                            break;
                        default:
                            builder.Append(r[c.ColumnName]);
                            break;
                    }
                    builder.Append("</td>");
                }

                builder.Append("\n </tr>\n");
            }
            #endregion

            builder.Append("</table>\n");

            //builder.Append("<script>" + Properties.Resources.tableSort1  + "</script>");

            builder.Append("</center>\n");

            return builder.ToString();
        }

        public static string HtmlTableBlocked(DataTable dt, int contentId = 0, bool isAdmin = false)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlSearchbar());

            builder.Append("<center class='w3-responsive'>\n");
            builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");

            #region Überschriften
            builder.Append(" <tr class='" + MyStyle.PanelLight + "'>\n");

            if (isAdmin)
            {
                builder.Append(" <th><i class='w3-xxlarge material-icons-outlined'>create</i></th>\n");
            }

            foreach (DataColumn c in dt.Columns)
            {
                builder.Append(" <th>");
                builder.Append(c.ColumnName);
                builder.Append("</th>\n");
            }

            builder.Append("\n </tr>\n");
            #endregion

            #region Inhalt
            foreach (DataRow r in dt.Rows)
            {
                if (contentId != 0 && contentId != int.Parse(r[dt.Columns["Id"]].ToString())) continue; // Wenn nur eine Id angezeigt werden soll

                builder.Append(" <tr class='myRow'>\n\t");

                if (isAdmin)
                {
                    builder.Append(" <td><input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>\n");
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

                            builder.Append(" <input class='w3-check' form='form1' name='" + c.ColumnName + "' type='checkbox' " + check + " " + ((contentId == 0) ? "disabled" : string.Empty) + ">\n");
                            break;
                        case "Beginn":
                        case "Ende":
                            builder.Append(" <select class='w3-select' form='form1' name='" + c.ColumnName + "' " + ((contentId == 0) ? "disabled" : string.Empty) + ">\n");

                            int current = int.Parse(r[c.ColumnName].ToString().Substring(0, 2));

                            if (contentId == 0)
                            {
                                builder.Append(" <option value='" + current + "' selected disabled>" + current + " Uhr</option>\n");
                            }
                            else
                            {
                                for (int i = 0; i < 24; i++)
                                {
                                    builder.Append(" <option value='" + i + "' " + ((current == i) ? "selected" : string.Empty) + ">" + i + " Uhr</option>\n");
                                }
                            }

                            builder.Append(" </select>\n");
                            break;
                        default:
                            builder.Append(" <div class='w3-margin'>");
                            builder.Append(r[c.ColumnName]);
                            builder.Append("</div>\n");
                            break;
                    }

                    builder.Append("</td>\n");
                }

                builder.Append("\n </tr>\n");
            }
            #endregion

            builder.Append("</table>\n");
            builder.Append("</center>\n");

            return builder.ToString();
        }

        public static string HtmlTableShift(DataTable dt, int shiftId = 0, int logedInUserId = 0, bool isAdmin = false)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(HtmlSearchbar());

            builder.Append("<center>\n");
            builder.Append(" <table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");

            #region Überschriften
            builder.Append(" <tr class='" + MyStyle.PanelLight + "'>\n\t");

            if (logedInUserId != 0)
            {
                builder.Append("<th><i class='w3-xxlarge material-icons-outlined'>create</i></th>");
            }

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
                    case "ContactId":
                        //nichts anzeigen
                        break;
                    default:
                        builder.Append(c.ColumnName);
                        break;
                }
                builder.Append("</th>");
            }

            builder.Append("\n </tr>\n");
            #endregion

            #region Inhalt
            foreach (DataRow r in dt.Rows)
            {
                if (shiftId != 0 && shiftId != int.Parse(r[dt.Columns["Id"]].ToString())) continue; // Wenn nur eine Id angezeigt werden soll

                builder.Append(" <tr class='myRow'>\n\t");

                if (logedInUserId != 0)
                {
                    int.TryParse(r[dt.Columns[0]].ToString(), out int tableIdColValue);
                    int.TryParse(r[dt.Columns[1]].ToString(), out int shiftContactId);

                    string disabled = "disabled";
                    string value = (tableIdColValue != 0) ? tableIdColValue.ToString() : "Datum_" + r[dt.Columns["Datum"]].ToString();
                    if (logedInUserId == shiftContactId || (tableIdColValue == 0 && logedInUserId != 0) || isAdmin) disabled = "";

                    builder.Append("<td>");
                    builder.Append("<input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + value + "' form='form1' onclick=\"w3.show('#Editor')\" " + disabled + ">");
                    builder.Append("</td>\n");
                }

                int procent;
                foreach (DataColumn c in dt.Columns)
                {
                    builder.Append("<td>");

                    switch (c.ColumnName)
                    {
                        case "ContactId":
                            builder.Append("<input type='hidden' value='" + r[c.ColumnName] + "'>");
                            break;
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
                            if (!int.TryParse(r[c.ColumnName].ToString(), out procent)) procent = 24; //* 100 / 24;
                            procent = procent * 100 / 24;
                            builder.Append("<div class='w3-cyan' style='width:240px;'>\n  <div class='w3-pale-blue' style='width:" + procent + "%'><span>" + r[c.ColumnName] + "&nbsp;Uhr</span></div>\n</div>\n");
                            break;
                        case "Ende":
                            int.TryParse(r[c.ColumnName].ToString(), out procent);
                            procent = procent * 100 / 24;
                            builder.Append("<div class='w3-pale-blue' style='width:240px;'>\n <div class='w3-cyan' style='width:" + procent + "%'>" + r[c.ColumnName] + "&nbsp;Uhr</div>\n</div>\n");
                            break;
                        default:
                            builder.Append(r[c.ColumnName]);
                            break;
                    }

                    builder.Append("</td>");
                }

                builder.Append("\n </tr>\n");
            }
            #endregion

            builder.Append(" </table>\n");
            builder.Append("</center>\n");

            return builder.ToString();
        }


        public static string HtmlFormLog(DateTime von, DateTime bis)
        {
            StringBuilder builder = new StringBuilder();
          
            builder.Append("<div class='w3-row w3-container'>\n");
            builder.Append(" <div class='w3-col l4 m3 s1'>\n");
            builder.Append("  &nbsp;\n");
            builder.Append(" </div>\n");
            builder.Append("  <div class='w3-col l2 m3 s5'>\n");
         // builder.Append("   <label>von:</label>\n");
            builder.Append("   <input form='form1' class='w3-input w3-border' type='date' name='von' value='" + von.ToString("yyyy-MM-dd") + "'>\n"); //
            builder.Append("  </div>\n");
            builder.Append("  <div class='w3-col l2 m3 s5'>\n");
         // builder.Append("   <label>bis:</label>\n");
            builder.Append("   <input form='form1' class='w3-input w3-border' type='date' name='bis' value='" + bis.ToString("yyyy-MM-dd") + "'>\n"); //
            builder.Append("  </div>\n");
            builder.Append("  <div class='w3-col l4 m3 s1'>\n");
         // builder.Append("   <label>X&nbsp;</label>\n");
            builder.Append("  <button form='form1' class='w3-bar-item w3-button " + MyStyle.Button + "' type='submit' formaction='/log'><i class='w3-xlarge material-icons-outlined'>schedule</i></button>\n");
            builder.Append("  </div>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlFormAccount(DataTable dtAccount, DataTable dtCompany, int showAccountId = 0, bool isAdmin = false)
        {
            string readOnly = isAdmin ? string.Empty : "readonly";
            string disabled = isAdmin ? string.Empty : "disabled";
            string styleReadOnly = isAdmin ? string.Empty : "w3-grey";

            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
            builder.Append("<h2 class='w3-center'>Benutzerkonto</h2>\n");

            if (isAdmin)
            {
                builder.Append("<script>\n");
                builder.Append("function chooseAccount(x) {\n");
                builder.Append(" x.action = '/account';\n");
                builder.Append(" x.submit();\n");
                builder.Append("}\n</script>\n");

                builder.Append("<div class='w3-row w3-section'>\n");
                builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>people</i></div>\n");

                builder.Append(" <div class='w3-rest'>\n");
                builder.Append("  <select form='form1' class='w3-select w3-border' name='selectedContactId' onchange='chooseAccount(this.form)'>\n");

                foreach (DataRow r in dtAccount.Rows)
                {
                    string selected = (int.Parse(r["ContactId"].ToString()) == showAccountId) ? "selected" : string.Empty;
                    builder.Append("   <option value='" + r["ContactId"] + "' " + selected + ">" + r["Name"].ToString() + "</option>\n");
                }

                builder.Append("  </select>\n");
                builder.Append(" </div>\n</div>\n");
            }

            int companyId = 0;
            foreach (DataRow r in dtAccount.Rows)
            {
                if (showAccountId != 0 && int.Parse(r[dtAccount.Columns["ContactId"]].ToString()) != showAccountId) continue; // nur einen von mehreren Accounts anzeigen

                foreach (DataColumn c in dtAccount.Columns)
                {
                    switch (c.ColumnName)
                    {
                        //ContactId,  Name, Passwort, CompanyId, Firma, Email, Telefon, SendSms, SendEmail, Max_Inaktivität 

                        case "ContactId":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>flag</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border w3-grey' type='text' placeholder='Laufende Nummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' readonly>\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Name":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>person</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Passwort":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>vpn_key</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border' type='password'  pattern='.{4,}' placeholder='Mind. 6 Zeichen; Gro&szlig;- und Kleinbuchstaben, Zahl' placeholder='Passwort' name='" + c.ColumnName + "' id='" + c.ColumnName + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "CompanyId":
                            companyId = int.Parse(r[c.ColumnName].ToString());
                            break;

                        case "Firma":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>work</i></div>\n");

                            builder.Append("  <button form='form1' type='submit' formaction='/company' style='max-width:60px' class='w3-col w3-button'>");
                            builder.Append("<i class='w3-xxlarge material-icons-outlined'>settings</i></button>\n");

                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <select form='form1' class='" + styleReadOnly + " w3-select w3-border' name='CompanyId' id='" + c.ColumnName + "' " + disabled + ">\n");

                            foreach (DataRow row in dtCompany.Rows)
                            {
                                if (isAdmin || (companyId.ToString() == row["Id"].ToString()))
                                {
                                    builder.Append("   <option value='" + row["Id"] + "' ");
                                    builder.Append((companyId.ToString() == row["Id"].ToString()) ? "selected" : string.Empty);
                                    builder.Append(">" + row["Name"].ToString() + "</option>\n");
                                }
                            }

                            builder.Append("  </select>\n");
                            builder.Append(" </div>\n</div>\n");
                            break;


                        case "Telefon":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>phone</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border' type='tel'  placeholder='Mobiltelefonnummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='+" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Email":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border' type='email' pattern='[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$' placeholder='Emailadresse' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "SendSms":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_phone</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-check w3-border w3-margin' type='checkbox' placeholder='SMS empfangen' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "SendEmail":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_mail</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-check w3-border w3-margin' type='checkbox' placeholder='Email empfangen' " +
                                           "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        case "Max_Inaktivität":
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>more_time</i></div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='" + styleReadOnly + " w3-input w3-border' type='number' min='0' step='8' placeholder='Maximale Inaktivität in Stunden' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' " + readOnly + ">\n");
                            builder.Append(" </div>\n</div>\n");
                            break;

                        default:
                            builder.Append("<div class='w3-row w3-section'>\n");
                            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>" + c.ColumnName + "</div>\n");
                            builder.Append(" <div class='w3-rest'>\n");
                            builder.Append("  <input form='form1' class='w3-input w3-border' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
                            builder.Append(" </div>\n</div>\n");
                            break;
                    }

                }
            }

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>&nbsp;</div>\n");
            builder.Append(" <div class='w3-rest'>\n");            
            builder.Append(" <button class='w3-button " + MyStyle.PanelLight + " w3-margin' type='reset' form='form1'>Zurücksetzen</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/account/update' >Ändern</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/account/create' " + disabled + ">Neu</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/account/delete' " + disabled + ">Löschen</button>\n");
            builder.Append(" </div>\n</div>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

        public static string HtmlFormShift(DateTime date, int shiftId, int shiftContactId = 0, bool isAdmin = false)
        {
            string name = "-KEIN NAME-";
            bool sendSms = false;
            bool sendEmail = false;
            int beginHour = 17;
            int endHour = 7;
            string disabled = "disabled";

            if (date == DateTime.MinValue)
                date = DateTime.Now.Date.AddDays(1);

            StringBuilder builder = new StringBuilder();

            builder.Append("<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
            builder.Append("<h2 class='w3-center'>Bereitschaft</h2>\n");

            #region Füllwerte ermitteln
            if (shiftId != 0)
            {
                //Bestandsdaten in Formular laden
                DataTable dtShift = Program.Sql.GetViewShift(shiftId);

                shiftContactId = int.Parse(dtShift.Rows[0]["ContactId"].ToString());
                name = dtShift.Rows[0]["Name"].ToString();
                sendSms = (int.Parse(dtShift.Rows[0]["SendSms"].ToString()) > 0);
                sendEmail = (int.Parse(dtShift.Rows[0]["SendEmail"].ToString()) > 0);
                date = DateTime.Parse(dtShift.Rows[0]["Datum"].ToString());
                beginHour = int.Parse(dtShift.Rows[0]["Beginn"].ToString());
                endHour = int.Parse(dtShift.Rows[0]["Ende"].ToString());
            }
            else if (shiftContactId == 0)
            {
                builder.Append(MelBoxWeb.HtmlAlert(2, "Kein gültiger Empfänger ausgewählt", "Die Bereitschaft kann nur einem gültigen Empänger zugeteilt werden."));
                return builder.ToString();
            }
            else
            {
                //neue Bereitschaftdaten erstellen
                DataTable dtContact = Program.Sql.GetViewContactInfo(shiftContactId);

                if (dtContact.Rows.Count > 0)
                {
                    name = dtContact.Rows[0]["Name"].ToString();
                    sendSms = (int.Parse(dtContact.Rows[0]["SendSms"].ToString()) > 0);
                    sendEmail = (int.Parse(dtContact.Rows[0]["SendEmail"].ToString()) > 0);
                    beginHour = MelBoxSql.ShiftStandardStartTime(date).Hour;
                    endHour = MelBoxSql.ShiftStandardEndTime(date).Hour;
                }
            }
            #endregion

            #region Formular
            //Kontakt-Auswahl für Admin
            if (isAdmin)
            {
                disabled = string.Empty;

                builder.Append("<script>\n");
                builder.Append("function myFunction(x, y) {\n");
                builder.Append(" document.getElementById('ContactId').value = x;\n");
                builder.Append(" document.getElementById('Name').value = document.getElementById('selectedContact').options[y].text;\n");
                builder.Append(" document.getElementById('form1').formaction = '/shift/edit'\n");
                builder.Append(" document.getElementById('form1').submit()\n");
                builder.Append("}\n</script>\n");

                builder.Append("<div class='w3-row w3-section'>\n");
                builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>people</i></div>\n");
                builder.Append(" <div class='w3-col w3-half'>\n");

                builder.Append("  <select form='form1' class='w3-input w3-border w3-margin-bottom' name='selectedContact' id='selectedContact' onchange='myFunction(this.value, this.selectedIndex)'>\n");

                DataTable dtContactSelection = Program.Sql.GetContactList();
                foreach (DataRow row in dtContactSelection.Rows)
                {
                    if (int.TryParse(row["ContactId"].ToString(), out int selectionContactId))
                    {
                        builder.Append("   <option value='" + selectionContactId + "' " + ((selectionContactId == shiftContactId) ? "selected" : string.Empty) + ">" + row["Name"] + "</option>\n"); //
                    }
                }
                builder.Append("  </select >\n");
                builder.Append(" </div>\n");
                builder.Append("</div>\n");
            }

            //Id
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>flag</i></div>\n");
            builder.Append(" <div class='w3-col l3 m4 s6'>\n");
            builder.Append("  <input form='form1' class='w3-input w3-border w3-grey' type='text' placeholder='Laufende Nummer' name='selectedRow' id='Nr' value='" + shiftId + "' readonly>\n");
            builder.Append(" </div>\n</div>\n");
            //ContactId         
            builder.Append("  <input form='form1' type='hidden' name='ContactId' id='ContactId' value='" + shiftContactId + "'>\n");
            //Name
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>person</i></div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append("  <input form='form1' class='w3-input w3-border w3-grey' type='text'  placeholder='Anzeigename' name='Name' id='Name' value='" + name + "' readonly>\n");
            builder.Append(" </div>\n</div>\n");

            //SendSms
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>smartphone</i></div>\n");
            builder.Append(" <div class='w3-col l3 m4 s6'>\n");
            builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border' type='checkbox' name='SendSms' id='SendSms' " + (sendSms ? "checked" : string.Empty) + " disabled>\n");
            builder.Append(" </div>\n</div>\n");
            //SendEmail
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
            builder.Append(" <div class='w3-col l3 m4 s6'>\n");
            builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border' type='checkbox' name='SendEmail' id='SendEmail' " + (sendEmail ? "checked" : string.Empty) + " disabled>\n");
            builder.Append(" </div>\n</div>\n");

            //Datum
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>today</i></div>\n");
            builder.Append(" <div class='w3-col l3 m4 s6'>\n"); ;
            builder.Append("  <input form='form1' class='w3-input w3-border' type='date'  placeholder='Beginndatum' name='Datum' id='Datum' " +
                           "min='" + DateTime.Now.ToString("yyyy-MM-dd") + "' value='" + date.ToString("yyyy-MM-dd") + "' autocomplete required>\n");
            builder.Append(" </div>\n");

            //Woche erstellen
            builder.Append(" <div class='w3-right-align w3-col l1 m1 s1'><i class='w3-xxlarge material-icons-outlined'>date_range</i></div>\n");
            builder.Append(" <div class='w3-rest w3-tooltip'>\n");
            builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border w3-col l1 m1 s1' type='checkbox' name='CreateWeekShift' id='CreateWeekShift' >\n");
            builder.Append("  <span class='w3-text w3-tag w3-teal'><b>Ganze Kalenderwoche erstellen</b></span>\n ");
            builder.Append(" </div>\n");
            builder.Append("</div>\n");

            //Beginn
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><span>Beginn</span><i class='w3-xxlarge material-icons-outlined'>hourglass_top</i></div>\n");
            builder.Append(" <div class='w3-col l1 m3 s6'>\n");
            builder.Append("  <input form='form1' class='w3-input w3-border' type='number' min='0' max='23' name='Beginn' id='Beginn' value='" + beginHour + "' >\n");
            builder.Append(" </div>\n</div>\n");
            //Ende
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><span>Ende</span><i class='w3-xxlarge material-icons-outlined'>hourglass_bottom</i></div>\n");
            builder.Append(" <div class='w3-col l1 m3 s6'>\n");
            builder.Append("  <input form='form1' class='w3-input w3-border' type='number' min='0' max='23' name='Ende' id='Ende' value='" + endHour + "' >\n");
            builder.Append(" </div>\n</div>\n");

            //Buttons
            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>&nbsp;</div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append(" <button class='w3-button " + MyStyle.PanelLight + " w3-margin' type='reset' form='form1'>Zurücksetzen</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/shift/update'>Übernehmen</button>\n");           
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/shift/delete' " + (shiftId==0 ? "disabled" : disabled ) + ">Löschen</button>\n"); //man kann nur löschen, wenn shiftId vorhanden
            builder.Append(" </div>\n</div>\n");
            builder.Append("</div>\n");
            #endregion

            return builder.ToString();
        }

        public static string HtmlFormCompany(DataTable dtCompany, int companyId, bool isAdmin)
        {
            string disabled = isAdmin ? string.Empty : "disabled";

            StringBuilder builder = new StringBuilder();

            builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
            builder.Append("<h2 class='w3-center'>Firmenkonto</h2>\n");

            #region Firmenauswahl
            if (isAdmin)
            {
                builder.Append("<div class='w3-row w3-section'>\n");
                builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>\n");
                builder.Append("<i class='w3-xxlarge material-icons-outlined'>list_alt</i>\n</div>\n");

                builder.Append(" <div class='w3-rest'>\n");
                builder.Append("<script>\n");
                builder.Append("function myFunction() {\n");
                //  builder.Append(" var myLink = '/company/' + document.getElementById('selectedCompany').value;\n");
                builder.Append(" var myLink = '/company';\n");
                builder.Append(" window.open(myLink, '_self');\n");
                builder.Append("}\n</script>\n");

                builder.Append("<script>\n");
                builder.Append("function myFunction(x) {\n");
                builder.Append(" document.getElementById('CompanyId').value = x;\n");
                //builder.Append(" document.getElementById('Name').value = document.getElementById('selectedContact').options[y].text;\n");
                builder.Append(" document.getElementById('form1').formaction = '/company'\n");
                builder.Append(" document.getElementById('form1').submit()\n");
                builder.Append("}\n</script>\n");

                builder.Append("  <select class='w3-select w3-border w3-pale-blue' id='selectedCompany'  onchange='myFunction(this.value)'>\n");

                foreach (DataRow row in dtCompany.Rows)
                {
                    builder.Append("   <option value='" + row["Id"] + "' ");
                    builder.Append((companyId.ToString() == row["Id"].ToString()) ? "selected" : string.Empty);
                    builder.Append(">" + row["Name"].ToString() + "</option>\n");
                }

                builder.Append("  </select>\n");
                builder.Append(" </div>\n</div>\n");
            }
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
                            c.ColumnName = "CompanyId";
                            icon = "flag";
                            placeholder = "Eindeutige Nummer";
                            inputReadonly = "readonly";
                            break;

                        case "Name":
                            icon = "local_offer";
                            placeholder = "Firmenname";
                            break;

                        case "Adresse":
                            icon = "location_on";
                            placeholder = "Adresse oder Werk";
                            break;

                        case "Ort":
                            icon = "location_city";
                            placeholder = "Ort";
                            break;

                        default:
                            icon = "info";
                            placeholder = c.ColumnName;
                            break;
                    }

                    builder.Append("<div class='w3-row w3-section'>\n");
                    builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>" + icon + "</i></div>\n");
                    builder.Append(" <div class='w3-rest'>\n");
                    builder.Append("  <input form='form1' class='w3-input w3-border " + (inputReadonly.Length > 0 ? "w3-grey" : string.Empty) + "' type='text' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' placeholder='" + placeholder + "' " + inputReadonly + ">\n");
                    builder.Append(" </div>\n</div>\n");
                }
                break;
            }

            builder.Append("<div class='w3-row w3-section'>\n");
            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>&nbsp;</div>\n");
            builder.Append(" <div class='w3-rest'>\n");
            builder.Append(" <button class='w3-button " + MyStyle.PanelLight + " w3-margin' type='reset' form='form1'>Zurücksetzen</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/company/update' " + disabled + ">Ändern</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/company/create' " + disabled + ">Neu</button>\n");
            builder.Append(" <button class='w3-button " + MyStyle.Button + " w3-margin' type='submit' form='form1' formaction='/company/delete' " + disabled + ">Löschen</button>\n");
            builder.Append(" </div>\n</div>\n");
            builder.Append("</div>\n");

            return builder.ToString();
        }

    }
}

    //    #region Werkzeuge
    //    public static string EncodeUmlaute(string input)
    //    {
    //        //&     &amp;   Zerstört Sonderzeichen in HTML!
    //        //"     &quot;
    //        //<     &lt;
    //        //>     &gt;
    //        //' 	&apos;
    //        return input.Replace("Ä", "&Auml;").Replace("Ö", "&Ouml;").Replace("Ü", "&Uuml;").Replace("ä", "&auml;").Replace("ö", "&ouml;").Replace("ü", "&uuml;").Replace("ß", "&szlig;");
    //    }

    //    public static string DecodeUmlaute(string input)
    //    {
    //        return input.Replace("%C4", "Ä").Replace("%D6", "Ö").Replace("%DC", "Ü").Replace("%E4", "ä").Replace("%F6", "ö").Replace("%FC", "ü")
    //            .Replace("%DF", "ß").Replace("%40", "@").Replace("%2B", "+").Replace("%26", "&").Replace("%22", "\"");
    //    }

    //    public static string GenerateID(int contactId)
    //    {
    //        string guid = Guid.NewGuid().ToString("N");

    //        while (MelBoxWeb.LogedInGuids.Count > 20) //nicht mehr als 20 Ids merken
    //        {
    //            MelBoxWeb.LogedInGuids.Remove(MelBoxWeb.LogedInGuids.FirstOrDefault().Key);
    //        }

    //        MelBoxWeb.LogedInGuids.Add(guid, contactId);
    //        return guid;
    //    }

    //    public static Dictionary<string, string> ReadPayload(string payload)
    //    {
    //        payload = payload ?? string.Empty;
    //        Dictionary<string, string> dict = new Dictionary<string, string>();

    //        payload = MelBoxWeb.DecodeUmlaute(payload);

    //        string[] args = payload.Split('&');

    //        foreach (string arg in args)
    //        {
    //            string[] items = arg.Split('=');

    //            if (items.Length > 1)
    //                dict.Add(items[0], items[1]);
    //        }

    //        return dict;
    //    }

    //    #endregion

    //    #region Bausteine

    //    public static string HtmlHead(string htmlTitle, string guid = "")
    //    {
    //        if (guid == null) guid = string.Empty;

    //        StringBuilder builder = new StringBuilder();
    //        builder.Append("<html>\n");
    //        builder.Append("<head>\n");
    //        builder.Append("<link rel='shortcut icon' href='https://www.kreutztraeger-kaeltetechnik.de/wp-content/uploads/2016/12/favicon.ico'>\n");
    //        builder.Append("<title>");
    //        builder.Append("MelBox2 - " + htmlTitle);
    //        builder.Append("</title>\n");
    //        builder.Append("<meta name='viewport' content='width=device-width, initial-scale=1'>\n");
    //        builder.Append("<link rel='stylesheet' href='https://www.w3schools.com/w3css/4/w3.css'>\n");
    //        builder.Append("<link rel='stylesheet' href='https://fonts.googleapis.com/icon?family=Material+Icons+Outlined'>\n");

    //        builder.Append("<style>\n");
    //        builder.Append(".w3-hidden {\n");
    //        builder.Append("  visibility: hidden;\n");
    //        builder.Append("}\n");
    //        builder.Append("</style>");

    //        builder.Append("<script src='https://www.w3schools.com/lib/w3.js'></script>");
    //        builder.Append("<script>\n");
    //        builder.Append("function readguid() {\n");
    //        // builder.Append(" alert('This message was triggered from the onload event');\n");

    //        if (guid.Length > 0)
    //        {
    //            builder.Append(" sessionStorage.setItem('guid','" + guid + "');\n ");
    //        }

    //        builder.Append(" if ( sessionStorage.getItem('guid') !== null) {\n");
    //        builder.Append("  var guid = sessionStorage.getItem('guid');");
    //        builder.Append("  document.getElementById('guid').value = guid;");
    //        // builder.Append("  document.getElementById('guid').value = sessionStorage.getItem('guid');");
    //        builder.Append("  document.getElementById('buttonin').href = '/in/' + guid;");
    //        builder.Append("  document.getElementById('buttonblocked').href = '/blocked/' + guid;");
    //        builder.Append("  document.getElementById('buttonaccount').href = '/account/' + guid;");
    //        builder.Append("  document.getElementById('buttonshift').href = '/shift/' + guid;");
    //        //builder.Append("  document.getElementById('buttonaccount').href = '/account/' + sessionStorage.getItem('guid');");
    //        //builder.Append("  document.getElementById('buttonshift').href = '/shift/' + sessionStorage.getItem('guid');");
    //        builder.Append("  w3.removeClass('.w3-disabled','w3-disabled'); \n");
    //        builder.Append("  w3.removeClass('.w3-hidden','w3-hidden'); \n");
    //        builder.Append(" }\n");
    //        builder.Append("}\n");
    //        builder.Append("</script>\n");

    //        builder.Append("</head>\n");
    //        builder.Append("<body onload='readguid()'>\n");
    //        builder.Append("<div class='w3-display-topright w3-opacity'>" + DateTime.Now + "</div>\n");

    //        builder.Append(HtmlMainMenu());

    //        builder.Append("<center>\n");
    //        builder.Append("<div class='w3-container w3-cyan w3-margin-bottom'>\n");
    //        builder.Append("<h1>MelBox2 - " + htmlTitle + "</h1>\n</div>\n\n");

    //        return builder.ToString();
    //    }

    //    private static string HtmlMainMenu()
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<div class='w3-bar w3-border'>\n");
    //        builder.Append("<a href='/' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></a>");
    //        builder.Append("<div class='w3-bar-item'></div>");

    //        builder.Append("<a href='/in' id='buttonin' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>drafts</i></a>");
    //        builder.Append("<a href='/out' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>forward_to_inbox</i></a>");
    //        builder.Append("<a href='/overdue' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>pending_actions</i></a>");

    //        builder.Append("<a href='/blocked' id='buttonblocked' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>notifications_off</i></a>");
    //        builder.Append("<a href='/shift' id='buttonshift' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>event_note</i></a>");
    //        builder.Append("<a href='/account' id='buttonaccount' class='w3-button w3-bar-item w3-disabled'><i class='w3-xxlarge material-icons-outlined'>assignment_ind</i></a>");

    //        builder.Append("<div class='w3-bar-item'></div>");
    //        builder.Append("<a href='/log' class='w3-button w3-bar-item'><i class='w3-xxlarge material-icons-outlined'>assignment</i></a>");

    //        builder.Append("</div>\n");

    //        return builder.ToString();
    //    }

    //    public static string HtmlFoot()
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("</center>\n");
    //        builder.Append("<div class='w3-container w3-cyan w3-margin-top w3-right-align '>\n");
    //        builder.Append(" <input id='guid' name='guid' form='form1' class='w3-cyan w3-text-light-blue w3-tiny w3-border-0' readonly>\n");
    //        builder.Append("</div>\n</body>\n</html>");
    //        return builder.ToString();
    //    }

    //    public static string HtmlLogin()
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<div class='w3-row w3-margin'>\n");
    //        builder.Append(" <div class='w3-container w3-quarter'></div>\n");
    //        builder.Append(" <div class='w3-card w3-half'>\n");
    //        builder.Append("  <div class='w3-container w3-cyan'>\n");
    //        builder.Append("    <h2>Login</h2>\n");
    //        builder.Append("  </div>\n");
    //        builder.Append("  <form class='w3-container' action='/' method='post'>\n");
    //        builder.Append("    <input style='display:none;' name='p' value='login'>\n");
    //        builder.Append("    <label class='w3-text-grey'><b>Benutzer</b></label>\n");
    //        builder.Append("    <input class='w3-input w3-border w3-sand' name='name' type='text' placeholder='Benutzername (von anderen sichtbar)' required></p>\n");
    //        builder.Append("    <label class='w3-text-grey'><b>Passwort</b></label>\n");
    //        builder.Append("    <input class='w3-input w3-border w3-sand' name='password' type='password' pattern='(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{ 6,}' placeholder='Mind. 6 Zeichen; Gro&szlig;- und Kleinbuchstaben, Zahl' required></p>\n");
    //        builder.Append("    <p>\n");
    //        builder.Append("    <button class='w3-button w3-teal'>Login</button>\n</p>\n");
    //        builder.Append("  </form>\n");
    //        builder.Append(" </div>\n</div>\n");

    //        return builder.ToString();
    //    }

    //    public static string HtmlAlert(int prio, string caption, string alarmText)
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        switch (prio)
    //        {
    //            case 1:
    //                builder.Append("<div class='w3-panel w3-pale-red'>\n");
    //                break;
    //            case 2:
    //                builder.Append("<div class='w3-panel w3-pale-yellow'>\n");
    //                break;
    //            case 3:
    //                builder.Append("<div class='w3-panel w3-pale-green'>\n");
    //                break;
    //            default:
    //                builder.Append("<div class='w3-panel w3-pale-blue'>\n");
    //                break;
    //        }

    //        builder.Append(" <h3>" + caption + "</h3>\n");
    //        builder.Append(" <p>" + alarmText + "</p>\n");
    //        builder.Append("</div>\n");

    //        return builder.ToString();
    //    }

    //    public static string HtmlAccordeonInfo(string caption, string infoText)
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<button onclick = \"myAccordion('accordion1')\"' class='w3-button w3-block w3-teal w3-left-align w3-margin-top'>Info</button>\n");
    //        builder.Append(" <div id='accordion1' class='w3-container w3-hide'>\n");
    //        builder.Append("  <div class='w3-panel w3-pale-blue'>\n");
    //        builder.Append("   <h3>" + caption + "</h3>\n");
    //        builder.Append("   <p>" + infoText + "</p>\n");
    //        builder.Append("  </div>\n");
    //        builder.Append(" </div>\n");
    //        builder.Append("</div>\n");

    //        builder.Append("<script>\n");
    //        builder.Append("function myAccordion(id) {\n");           
    //        builder.Append("  var x = document.getElementById(id);\n");
    //        builder.Append("  if (x.className.indexOf('w3-show') == -1)\n");
    //        builder.Append("  {\n");
    //        builder.Append("    x.className += 'w3-show';\n");
    //        builder.Append("  }\n");
    //        builder.Append("  else\n");
    //        builder.Append("  {\n");
    //        builder.Append("    x.className = x.className.replace('w3-show', '');\n");
    //        builder.Append("  }\n");
    //        builder.Append("}\n");
    //        builder.Append("</script>\n");

    //        return builder.ToString();
    //    }

    //    #endregion

    //    #region Tabellen
    //    public static string HtmlTablePlain(DataTable dt, int logedInUserId = 0)
    //    {
    //        //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserId);
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<div class='w3-container'>\n");
    //        builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
    //        builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
    //        builder.Append("</div>");

    //        builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
    //        builder.Append("<tr class='w3-teak'>\n\t");

    //        if (isAdmin)
    //        {
    //            builder.Append("<th><i class='w3-xxlarge material-icons-outlined'>create</i></th>");
    //        }

    //        foreach (DataColumn c in dt.Columns)
    //        {
    //            builder.Append("<th>");
    //            builder.Append(c.ColumnName);
    //            builder.Append("</th>");
    //        }

    //        builder.Append("\n</tr>\n");

    //        foreach (DataRow r in dt.Rows)
    //        {
    //            builder.Append("<tr class='myRow'>\n\t");

    //            if (isAdmin)
    //            {
    //                builder.Append("<td><input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>");
    //            }

    //            foreach (DataColumn c in dt.Columns)
    //            {
    //                builder.Append("<td>");
    //                builder.Append(r[c.ColumnName]);
    //                builder.Append("</td>");
    //            }
    //            builder.Append("\n</tr>\n");
    //        }
    //        builder.Append("</table>\n");

    //        return builder.ToString();
    //    }

    //    private static string HtmlTableShift(DataTable dt, int shiftId = 0, int logedInUserId = 0)
    //    {
    //        try
    //        {
    //            StringBuilder builder = new StringBuilder();

    //            builder.Append("<div class='w3-container'>\n");
    //            builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
    //            builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
    //            builder.Append("</div>");

    //            builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
    //            builder.Append("<tr class='w3-teak'>\n\t");

    //            if (logedInUserId != 0)
    //            {
    //                builder.Append("<th><i class='w3-xxlarge material-icons-outlined'>create</i></th>");
    //            }

    //            foreach (DataColumn c in dt.Columns)
    //            {
    //                builder.Append("<th>");
    //                switch (c.ColumnName)
    //                {
    //                    case "SendSms":
    //                        builder.Append("<i class='w3-xxlarge material-icons-outlined'>smartphone</i>");
    //                        break;
    //                    case "SendEmail":
    //                        builder.Append("<i class='w3-xxlarge material-icons-outlined'>email</i>");
    //                        break;
    //                    case "ContactId":
    //                        //nichts anzeigen
    //                        break;
    //                    default:
    //                        builder.Append(c.ColumnName);
    //                        break;
    //                }

    //                builder.Append("</th>");
    //            }
    //            builder.Append("\n</tr>\n");

    //            int procent;
    //            bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserId);

    //            foreach (DataRow r in dt.Rows)
    //            {
    //                if (shiftId != 0 && shiftId != int.Parse(r[dt.Columns["Id"]].ToString())) continue; // Wenn nur eine Id angezeigt werden soll

    //                builder.Append("<tr class='myRow'>\n\t");

    //                if (logedInUserId != 0)
    //                {
    //                    int.TryParse(r[dt.Columns[0]].ToString(), out int tableIdColValue);
    //                    int.TryParse(r[dt.Columns[1]].ToString(), out int shiftContactId);

    //                    string disabled = "disabled";
    //                    string value = (tableIdColValue != 0) ? tableIdColValue.ToString() : "Datum_" + r[dt.Columns["Datum"]].ToString();
    //                    if (logedInUserId == shiftContactId || (tableIdColValue == 0 && logedInUserId != 0) || isAdmin) disabled = "";

    //                    builder.Append("<td>");
    //                    builder.Append("<input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + value + "' form='form1' onclick=\"w3.show('#Editor')\" " + disabled + ">");
    //                    builder.Append("</td>\n");
    //                }

    //                foreach (DataColumn c in dt.Columns)
    //                {
    //                    builder.Append("<td>");
    //                    switch (c.ColumnName)
    //                    {
    //                        case "ContactId":
    //                            builder.Append("<input type='hidden' value='" + r[c.ColumnName] + "'>");
    //                            break;
    //                        case "SendSms":
    //                            builder.Append("<input class='w3-check w3-center' type='checkbox' name='SendSms' ");
    //                            builder.Append((int.Parse(r[c.ColumnName].ToString()) > 0) ? "checked" : string.Empty);
    //                            builder.Append(" disabled>");
    //                            break;
    //                        case "SendEmail":
    //                            builder.Append("<input class='w3-check w3-center' type='checkbox' name='SendEmail' ");
    //                            builder.Append((int.Parse(r[c.ColumnName].ToString()) > 0) ? "checked" : string.Empty);
    //                            builder.Append(" disabled>");
    //                            break;
    //                        case "Beginn":
    //                            if (!int.TryParse(r[c.ColumnName].ToString(), out procent)) procent = 24; //* 100 / 24;
    //                            procent = procent * 100 / 24;
    //                            builder.Append("<div class='w3-cyan' style='width:240px;'>\n  <div class='w3-pale-blue' style='width:" + procent + "%'><span>" + r[c.ColumnName] + "&nbsp;Uhr</span></div>\n</div>\n");
    //                            break;
    //                        case "Ende":
    //                            int.TryParse(r[c.ColumnName].ToString(), out procent);
    //                            procent = procent * 100 / 24;
    //                            builder.Append("<div class='w3-pale-blue' style='width:240px;'>\n <div class='w3-cyan' style='width:" + procent + "%'>" + r[c.ColumnName] + "&nbsp;Uhr</div>\n</div>\n");
    //                            break;
    //                        //case "Datum":
    //                        //    builder.Append("<input class='w3-border-0' form='form1' type='text' name='selectedDate' value='" + r[c.ColumnName] + "' readonly>\n");
    //                        //    break;
    //                        default:
    //                            builder.Append(r[c.ColumnName]);
    //                            break;

    //                    }
    //                    builder.Append("</td>\n");
    //                }
    //                builder.Append("\n</tr>\n");
    //            }
    //            builder.Append("</table>\n");

    //            return builder.ToString();
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //    }

    //    private static string HtmlTableBlocked(DataTable dt, int contentId = 0, int logedInUserId = 0)
    //    {
    //        //Quelle: https://stackoverflow.com/questions/19682996/datatable-to-html-table

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserId);

    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<div class='w3-container'>\n");
    //        builder.Append(" <div class='w3-third'>&nbsp;</div>\n");
    //        builder.Append(" <input oninput=\"w3.filterHTML('#myTable', '.myRow', this.value)\" class='w3-input w3-third w3-margin w3-border w3-round-large' placeholder='Suche nach..'>\n");
    //        builder.Append("</div>");

    //        builder.Append("<table class='w3-table-all w3-hoverable w3-cell' id='myTable'>\n");
    //        builder.Append("<tr class='w3-teak'>\n\t");

    //        if (isAdmin)
    //        {
    //            builder.Append("<th><i class='w3-xxlarge material-icons-outlined'>create</i></th>");
    //        }

    //        foreach (DataColumn c in dt.Columns)
    //        {
    //            builder.Append("<th>");
    //            builder.Append(c.ColumnName);
    //            builder.Append("</th>");
    //        }
    //        builder.Append("\n</tr>\n");

    //        foreach (DataRow r in dt.Rows)
    //        {
    //            if (contentId != 0 && contentId != int.Parse(r[dt.Columns["Id"]].ToString())) continue; // Wenn nur eine Id angezeigt werden soll

    //            builder.Append("<tr class='myRow'>\n\t");

    //            if (isAdmin)
    //            {
    //                builder.Append("<td><input class='w3-radio w3-hidden' type='radio' name='selectedRow' value='" + r[dt.Columns[0]] + "' form='form1' onclick=\"w3.show('#Editor')\"></td>");
    //               // builder.Append("<input type='hidden' name='' value>");
    //            }

    //            foreach (DataColumn c in dt.Columns)
    //            {
    //                builder.Append("<td>");
    //                switch (c.ColumnName)
    //                {

    //                    case "So":
    //                    case "Mo":
    //                    case "Di":
    //                    case "Mi":
    //                    case "Do":
    //                    case "Fr":
    //                    case "Sa":
    //                        string check = string.Empty;
    //                        if (r[c.ColumnName].ToString() == "1")
    //                            check = "checked='checked' ";

    //                        builder.Append("<input class='w3-check' form='form1' name='" + c.ColumnName + "' type='checkbox' " + check + " " + ((contentId == 0) ? "disabled" : string.Empty) + ">\n");
    //                        break;
    //                    case "Beginn":
    //                    case "Ende":
    //                        builder.Append("<select class='w3-select' form='form1' name='" + c.ColumnName + "' " + ((contentId == 0) ? "disabled" : string.Empty) + ">\n");

    //                        int current = int.Parse(r[c.ColumnName].ToString().Substring(0, 2));

    //                        if (contentId == 0)
    //                        {
    //                            builder.Append("<option value='" + current + "' selected disabled>" + current + " Uhr</option>\n");
    //                        }
    //                        else
    //                        {
    //                            for (int i = 0; i < 24; i++)
    //                            {
    //                                builder.Append("<option value='" + i + "' " + ((current == i) ? "selected" : string.Empty) + ">" + i + " Uhr</option>\n");
    //                            }
    //                        }

    //                        builder.Append("</select>\n");
    //                        break;
    //                    default:
    //                        builder.Append("<div class='w3-margin'>");
    //                        builder.Append(r[c.ColumnName]);
    //                        builder.Append("</div>");
    //                        break;
    //                }
    //                builder.Append("</td>");
    //            }
    //            builder.Append("\n</tr>\n");
    //        }
    //        builder.Append("</table>\n");

    //        return builder.ToString();
    //    }

    //    #endregion

    //    #region Formulare
    //    /// <summary>
    //    /// Wird als Rückfrage eingeblendet, sendet das Html-Form 'form1' an einen Link
    //    /// </summary>
    //    /// <param name="action">Kombination aus Link, Button-Text</param>
    //    /// <returns>Html-Baustein</returns>
    //    public static string HtmlEditor(Dictionary<string, string> action)
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("<div id='Editor' class='w3-modal'>\n"); //
    //        builder.Append("  <div class='w3-modal-content w3-card-4' style='max-width:600px'>\n");
    //        builder.Append("  <div class='w3-container'>\n");
    //        builder.Append("  <div class='w3-margin'><p>Optionen</p></div>\n");
    //        builder.Append("   <span onclick=\"w3.hide('#Editor')\" class='w3-button w3-margin-bottom w3-display-topright'><i class='w3-xxlarge material-icons-outlined'>close</i></span>");
    //        builder.Append("   <form id='form1' method='post' class='w3-margin' action=''>\n");

    //        foreach (var path in action.Keys)
    //        {
    //            builder.Append("    <button class='w3-button w3-block w3-teal w3-section w3-padding-large w3-margin w3-disabled' ");
    //            builder.Append("formaction='" + path + "' type='submit'>" + action[path] + "</button>");
    //        }

    //        builder.Append("   </form>\n");
    //        builder.Append("  </div>\n");
    //        builder.Append("  </div>\n");
    //        builder.Append("</div>\n");

    //        return builder.ToString();
    //    }

    //    private static string HtmlFormAccount(DataTable dtAccount, DataTable dtCompany, bool isAdmin = false)
    //    {
    //        string readOnly = isAdmin ? string.Empty : "readonly";
    //        string styleReadOnly = isAdmin ? string.Empty : "w3-grey";

    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
    //        builder.Append("<h2 class='w3-center'>Benutzerkonto</h2>\n");

    //        int companyId = 0;
    //        foreach (DataRow r in dtAccount.Rows)
    //        {
    //            foreach (DataColumn c in dtAccount.Columns)
    //            {
    //                switch (c.ColumnName)
    //                {
    //                    //ContactId,  Name, Passwort, CompanyId, Firma, Email, Telefon, SendSms, SendEmail, Max_Inaktivität 

    //                    case "ContactId":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>flag</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-grey w3-disabled' type='text' placeholder='Laufende Nummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' readonly>\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "Name":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>person</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "Passwort":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>vpn_key</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='password'  placeholder='Passwort' name='" + c.ColumnName + "' id='" + c.ColumnName + "' >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "CompanyId":
    //                        companyId = int.Parse(r[c.ColumnName].ToString());
    //                        break;

    //                    case "Firma":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>work</i></div>\n");

    //                        builder.Append("<script>\n");
    //                        builder.Append("function myFunction() {\n");
    //                        //builder.Append(" alert('Setze Wert ');\n");
    //                        builder.Append(" document.getElementById('companyId2').value = document.getElementById('Firma').value;\n");
    //                        builder.Append(" document.getElementById('guid2').value = document.getElementById('guid').value;\n");
    //                        builder.Append("}\n</script>\n");

    //                        builder.Append("<form id='companyForm' action='/company' method='post'>\n");
    //                        builder.Append("  <input form='companyForm' type='hidden' id='companyId2' name='companyId'>");
    //                        builder.Append("  <input form='companyForm' type='hidden' id='guid2' name='guid'>");
    //                        builder.Append("  <input form='companyForm' type='submit' style='max-width:60px' class='w3-col w3-button' value='TEST'>"); //<i class='w3-xlarge material-icons-outlined'>settings</i></button>

    //                        builder.Append("</form>\n");


    //                       builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <select form='form1' class='" + styleReadOnly + " w3-select w3-border w3-disabled' name='CompanyId' id='" + c.ColumnName + "'  onchange='myFunction()' " + readOnly + ">\n");

    //                        foreach (DataRow row in dtCompany.Rows)
    //                        {
    //                            builder.Append("   <option value='" + row["Id"] + "' ");
    //                            builder.Append((companyId.ToString() == row["Id"].ToString()) ? "selected" : string.Empty);
    //                            builder.Append(">" + row["Name"].ToString() + "</option>\n");
    //                        }

    //                        builder.Append("  </select>\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;


    //                    case "Telefon":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>phone</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='tel'  placeholder='Mobiltelefonnummer' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='+" + r[c.ColumnName] + "' >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "Email":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='email' pattern='[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$' placeholder='Emailadresse' " +
    //                                       "name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "SendSms":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_phone</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-check w3-border w3-margin w3-disabled' type='checkbox' placeholder='SMS empfangen' " +
    //                                       "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "SendEmail":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>contact_mail</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-check w3-border w3-margin w3-disabled' type='checkbox' placeholder='Email empfangen' " +
    //                                       "name='" + c.ColumnName + "' id='" + c.ColumnName + "' " + (int.Parse(r[c.ColumnName].ToString()) != 0 ? "checked" : string.Empty) + " >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    case "Max_Inaktivität":
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>more_time</i></div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='" + styleReadOnly + "w3-input w3-border w3-disabled' type='number' min='0' step='8' placeholder='Maximale Inaktivität in Stunden' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' " + readOnly + ">\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;

    //                    default:
    //                        builder.Append("<div class='w3-row w3-section'>\n");
    //                        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>" + c.ColumnName + "</div>\n");
    //                        builder.Append(" <div class='w3-rest'>\n");
    //                        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='text'  placeholder='Anzeigename' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' >\n");
    //                        builder.Append(" </div>\n</div>\n");
    //                        break;
    //                }

    //            }
    //        }

    //        builder.Append(" <input type='reset' form='form1' class='w3-button w3-block w3-section w3-pale-blue w3-ripple w3-padding w3-half'>\n");
    //        builder.Append(" <button class='w3-button w3-block w3-section w3-teal w3-ripple w3-padding w3-half' onclick =\"document.getElementById('Editor').style.display = 'block'\">Ändern</button>\n");
    //        builder.Append("</div>\n<center>\n");

    //        return builder.ToString();
    //    }

    //    private static string HtmlFormCompany(DataTable dtCompany, int companyId)
    //    {
    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
    //        builder.Append("<h2 class='w3-center'>Firmenkonto</h2>\n");

    //        #region Firmenauswahl

    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'>\n");
    //        builder.Append("<i class='w3-xxlarge material-icons-outlined'>list_alt</i>\n</div>\n");

    //        builder.Append(" <div class='w3-rest'>\n");
    //        builder.Append("<script>\n");
    //        builder.Append("function myFunction() {\n");
    //        builder.Append(" var myLink = '/company/' + document.getElementById('selectedCompany').value;\n");
    //        builder.Append(" window.open(myLink, '_self');\n");
    //        builder.Append("}\n</script>\n");
    //        builder.Append("  <select class='w3-select w3-border w3-pale-blue w3-disabled' id='selectedCompany'  onchange='myFunction()'>\n");

    //        foreach (DataRow row in dtCompany.Rows)
    //        {
    //            builder.Append("   <option value='" + row["Id"] + "' ");
    //            builder.Append((companyId.ToString() == row["Id"].ToString()) ? "selected" : string.Empty);
    //            builder.Append(">" + row["Name"].ToString() + "</option>\n");
    //        }

    //        builder.Append("  </select>\n");
    //        builder.Append(" </div>\n</div>\n");
    //        #endregion

    //        foreach (DataRow r in dtCompany.Rows)
    //        {
    //            if (companyId.ToString() != r["Id"].ToString()) continue;

    //            foreach (DataColumn c in dtCompany.Columns)
    //            {
    //                string icon;
    //                string placeholder;
    //                string inputReadonly = string.Empty;

    //                switch (c.ColumnName)
    //                {
    //                    case "Id":
    //                        icon = "flag";
    //                        placeholder = "Eindeutige Nummer";
    //                        inputReadonly = "readonly";
    //                        break;

    //                    case "Name":
    //                        icon = "local_offer";
    //                        placeholder = "Firmenname";
    //                        break;

    //                    case "Adresse":
    //                        icon = "location_on";
    //                        placeholder = "Adresse oder Werk";
    //                        break;

    //                    case "Ort":
    //                        icon = "location_city";
    //                        placeholder = "Ort";
    //                        break;

    //                    default:
    //                        icon = "info";
    //                        placeholder = c.ColumnName;
    //                        break;
    //                }

    //                builder.Append("<div class='w3-row w3-section'>\n");
    //                builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>" + icon + "</i></div>\n");
    //                builder.Append(" <div class='w3-rest'>\n");
    //                builder.Append("  <input form='form1' class='w3-input w3-border " + (inputReadonly.Length > 0 ? "w3-grey" : string.Empty) + " w3-disabled' type='text' name='" + c.ColumnName + "' id='" + c.ColumnName + "' value='" + r[c.ColumnName] + "' placeholder='" + placeholder + "' " + inputReadonly + ">\n");
    //                builder.Append(" </div>\n</div>\n");
    //            }
    //            break;
    //        }

    //        builder.Append(" <div class='w3-quarter'>&nbsp;</div>\n");
    //        builder.Append(" <input type='reset' form='form1' class='w3-button w3-block w3-section w3-pale-blue w3-ripple w3-padding w3-quarter'>\n");
    //        builder.Append(" <button class='w3-button w3-block w3-section w3-teal w3-ripple w3-padding w3-quarter' onclick =\"document.getElementById('Editor').style.display = 'block'\">Ändern</button>\n");
    //        builder.Append("</div>\n<center>\n");

    //        return builder.ToString();
    //    }

    //    public static string HtmlFormShift(DateTime date, int shiftId, int shiftContactId = 0)
    //    {
    //        string name = "-KEIN NAME-";
    //        bool sendSms = false;
    //        bool sendEmail = false;
    //        //DateTime date = DateTime.Now.Date.AddDays(1);
    //        int beginHour = 17;
    //        int endHour = 7;

    //        if (date == DateTime.MinValue)
    //            date = DateTime.Now.Date.AddDays(1);

    //        StringBuilder builder = new StringBuilder();

    //        builder.Append("</center>\n<div class='w3-card w3-container w3-light-grey w3-text-teal w3-margin'>\n");
    //        builder.Append("<h2 class='w3-center'>Bereitschaft</h2>\n");

    //        #region Füllwerte ermitteln
    //        if (shiftId != 0)
    //        {
    //            //Bestandsdaten in Formular laden
    //            DataTable dtShift = Program.Sql.GetViewShift(shiftId);

    //            shiftContactId = int.Parse(dtShift.Rows[0]["ContactId"].ToString());
    //            name = dtShift.Rows[0]["Name"].ToString();
    //            sendSms = (int.Parse(dtShift.Rows[0]["SendSms"].ToString()) > 0);
    //            sendEmail = (int.Parse(dtShift.Rows[0]["SendEmail"].ToString()) > 0);
    //            date = DateTime.Parse(dtShift.Rows[0]["Datum"].ToString());
    //            beginHour = int.Parse(dtShift.Rows[0]["Beginn"].ToString());
    //            endHour = int.Parse(dtShift.Rows[0]["Ende"].ToString());
    //        }
    //        else if (shiftContactId == 0)
    //        {
    //            //ungültig
    //            builder.Append(MelBoxWeb.HtmlAlert(2, "Kein gültiger Empfänger ausgewählt", "Die Bereitschaft kann nur einem gültigen Empänger zugeteilt werden."));
    //            return builder.ToString();
    //        }
    //        else
    //        {
    //            //neue Bereitschaftdaten erstellen
    //            DataTable dtContact = Program.Sql.GetViewContactInfo(shiftContactId);

    //            if (dtContact.Rows.Count > 0)
    //            {                    
    //                name = dtContact.Rows[0]["Name"].ToString();
    //                sendSms = (int.Parse(dtContact.Rows[0]["SendSms"].ToString()) > 0);
    //                sendEmail = (int.Parse(dtContact.Rows[0]["SendEmail"].ToString()) > 0);
    //                beginHour = MelBoxSql.ShiftStandardStartTime(date).Hour;
    //                endHour = MelBoxSql.ShiftStandardEndTime(date).Hour;
    //            }
    //        }
    //        #endregion

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(shiftContactId);

    //        #region Formular
    //        //Kontakt-Auswahl für Admin
    //        if (isAdmin)
    //        {
    //            builder.Append("<script>\n");
    //            builder.Append("function myFunction(x, y) {\n");
    //            builder.Append(" document.getElementById('ContactId').value = x;\n");
    //            builder.Append(" document.getElementById('Name').value = document.getElementById('selectedContact').options[y].text;\n");
    //            builder.Append("}\n</script>\n");
    //            builder.Append("<div class='w3-row w3-section'>\n");
    //            builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>people</i></div>\n");
    //            builder.Append(" <div class='w3-col l3 m4 s6'>\n");

    //            builder.Append("  <select form='form1' class='w3-input w3-border w3-margin w3-disabled' name='selectedContact' id='selectedContact' onchange='myFunction(this.value, this.selectedIndex)'>\n");
    //            //builder.Append("   <option value='0' selected disabled>-keine Auswahl-</option>\n");
    //            DataTable dtContactSelection = Program.Sql.GetContactList();
    //            foreach (DataRow row in dtContactSelection.Rows)
    //            {
    //                if (int.TryParse(row["ContactId"].ToString(), out int selectionContactId))
    //                {
    //                    builder.Append("   <option value='" + selectionContactId + "' " + ((selectionContactId == shiftContactId) ? "selected" : string.Empty) + ">" + row["Name"] + "</option>\n"); //
    //                }
    //            }
    //            builder.Append("  </ select >\n");
    //            builder.Append(" </div>\n</div>\n");
    //        }

    //        //Id
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>flag</i></div>\n");
    //        builder.Append(" <div class='w3-col l3 m4 s6'>\n");
    //        builder.Append("  <input form='form1' class='w3-input w3-border w3-grey w3-disabled' type='text' placeholder='Laufende Nummer' name='selectedRow' id='Nr' value='" + shiftId + "' readonly>\n");
    //        builder.Append(" </div>\n</div>\n");
    //        //ContactId         
    //        builder.Append("  <input form='form1' type='hidden' name='ContactId' id='ContactId' value='" + shiftContactId + "'>\n");
    //        //Name
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>person</i></div>\n");
    //        builder.Append(" <div class='w3-rest'>\n");
    //        builder.Append("  <input form='form1' class='w3-input w3-border w3-grey w3-disabled' type='text'  placeholder='Anzeigename' name='Name' id='Name' value='" + name + "' readonly>\n");
    //        builder.Append(" </div>\n</div>\n");

    //        //SendSms
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>smartphone</i></div>\n");
    //        builder.Append(" <div class='w3-col l3 m4 s6'>\n");
    //        builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border w3-disabled' type='checkbox' name='SendSms' id='SendSms' " + (sendSms ? "checked" : string.Empty) + " disabled>\n");
    //        builder.Append(" </div>\n</div>\n");
    //        //SendEmail
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>email</i></div>\n");
    //        builder.Append(" <div class='w3-col l3 m4 s6'>\n");
    //        builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border w3-disabled' type='checkbox' name='SendEmail' id='SendEmail' " + (sendEmail ? "checked" : string.Empty) + " disabled>\n");
    //        builder.Append(" </div>\n</div>\n");

    //        //Datum
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><i class='w3-xxlarge material-icons-outlined'>today</i></div>\n");
    //        builder.Append(" <div class='w3-col l3 m4 s6'>\n"); ;
    //        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='date'  placeholder='Beginndatum' name='Datum' id='Datum' " +
    //                       "min='" + DateTime.Now.ToString("yyyy-MM-dd") + "' value='" + date.ToString("yyyy-MM-dd") + "' autocomplete required>\n");
    //        builder.Append(" </div>\n");

    //        //Woche erstellen
    //        builder.Append(" <div class='w3-right-align w3-col l1 m1 s1'><i class='w3-xxlarge material-icons-outlined'>date_range</i></div>\n");
    //        builder.Append(" <div class='w3-rest w3-tooltip'>\n");
    //        builder.Append("  <input form='form1' class='w3-check w3-center w3-margin w3-border w3-disabled w3-col l1 m1 s1' type='checkbox' name='CreateWeekShift' id='CreateWeekShift' >\n");
    //        builder.Append("  <span class='w3-text w3-tag w3-teal'><b>Ganze Kalenderwoche erstellen</b></span>\n ");
    //        builder.Append(" </div>\n");
    //        builder.Append("</div>\n");

    //        //Beginn
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><span>Beginn</span><i class='w3-xxlarge material-icons-outlined'>hourglass_top</i></div>\n");
    //        builder.Append(" <div class='w3-col l1 m3 s6'>\n");
    //        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='number' min='0' max='23' name='Beginn' id='Beginn' value='" + beginHour + "' >\n");
    //        builder.Append(" </div>\n</div>\n");
    //        //Ende
    //        builder.Append("<div class='w3-row w3-section'>\n");
    //        builder.Append(" <div class='w3-right-align w3-col l1 m2 s3'><span>Ende</span><i class='w3-xxlarge material-icons-outlined'>hourglass_bottom</i></div>\n");
    //        builder.Append(" <div class='w3-col l1 m3 s6'>\n");
    //        builder.Append("  <input form='form1' class='w3-input w3-border w3-disabled' type='number' min='0' max='23' name='Ende' id='Ende' value='" + endHour + "' >\n");
    //        builder.Append(" </div>\n</div>\n");

    //        builder.Append(" <div class='w3-quarter'>&nbsp;</div>\n");
    //        builder.Append(" <input type='reset' form='form1' class='w3-button w3-block w3-section w3-pale-blue w3-ripple w3-padding w3-quarter'>\n");
    //        builder.Append(" <button class='w3-button w3-block w3-section w3-teal w3-ripple w3-padding w3-quarter' onclick =\"document.getElementById('Editor').style.display = 'block'\">Übernehmen</button>\n");
    //        builder.Append("</div>\n<center>\n");

    //        #endregion

    //        return builder.ToString();
    //    }

    //    #endregion

    //    #region Vorgefertigte Blöcke

    //    /// <summary>
    //    /// Zusammengefasste Templates für geblockte Nachrichten
    //    /// </summary>
    //    /// <param name="dt"></param>
    //    /// <returns></returns>
    //    public static string HtmlUnitBlocked(DataTable dt, int contendId = 0, int logedInUserid = 0)
    //    {
    //        Dictionary<string, string> action = new Dictionary<string, string>();

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserid);

    //        if (isAdmin)
    //        {
    //            if (contendId == 0)
    //                action.Add("/blocked/update", "Sperrzeit der Nachricht bearbeiten");
    //            else
    //                action.Add("/blocked/update", "Bearbeitete Zeiten speichern");

    //            action.Add("/blocked/delete", "Aus Sperrliste entfernen");
    //        }

    //        StringBuilder builder = new StringBuilder();
    //        builder.Append(HtmlTableBlocked(dt, contendId, logedInUserid));
    //        builder.Append(HtmlEditor(action));

    //        return builder.ToString();
    //    }

    //    public static string HtmlUnitAccount(int contactId)
    //    {
    //        Dictionary<string, string> action = new Dictionary<string, string>();            
    //        action.Add("/account/update", "Änderungen an Kontakt speichern");

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(contactId);

    //        if (isAdmin)
    //        {
    //            action.Add("/account/create", "Neuen Kontakt mit diesen Angaben einrichten");
    //            action.Add("/account/delete", "Diesen Kontakt löschen");
    //        }

    //        DataTable dt = Program.Sql.GetViewContactInfo(contactId);
    //        DataTable dtCompany = Program.Sql.GetCompany();

    //        StringBuilder builder = new StringBuilder();
    //        builder.Append(MelBoxWeb.HtmlFormAccount(dt, dtCompany, isAdmin));
    //        builder.Append(MelBoxWeb.HtmlEditor(action));        

    //        return builder.ToString();
    //    }

    //    public static string HtmlUnitCompany(int companyId, int logedInUserId = 0)
    //    {
    //        Dictionary<string, string> action = new Dictionary<string, string>();
    //        bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserId);

    //        if (isAdmin)
    //        {
    //            action.Add("/company/create", "Firma neu anlegen");
    //            action.Add("/company/update", "Firmeninformationen ändern");
    //            action.Add("/company/delete", "Firma löschen");
    //        }

    //        DataTable dtCompany = Program.Sql.GetCompany();

    //        StringBuilder builder = new StringBuilder();
    //        builder.Append(MelBoxWeb.HtmlFormCompany(dtCompany, companyId));
    //        builder.Append(MelBoxWeb.HtmlEditor(action));        

    //        return builder.ToString();
    //    }

    //    public static string HtmlUnitShift(DataTable dt, int logedInUserId = 0)
    //    {
    //        Dictionary<string, string> action = new Dictionary<string, string>
    //            {
    //                { "/shift/create", "Bereitschaft neu anlegen" },
    //                { "/shift/update", "Bereitschaft ändern" }
    //            };

    //        bool isAdmin = MelBoxSql.AdminIds.Contains(logedInUserId);

    //        if (isAdmin)
    //        {
    //            action.Add("/shift/delete", "Bereitschaft löschen");
    //        }

    //        StringBuilder builder = new StringBuilder();
    //        builder.Append(HtmlTableShift(dt, 0, logedInUserId));

    //        builder.Append(HtmlEditor(action));

    //        return builder.ToString();
    //    }
    //    #endregion
    //}



