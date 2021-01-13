using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxSql
    {
        public bool DeleteMessageBlocked(int msgId)
        {
            try
            {
                const string query = "DELETE FROM BlockedMessages WHERE Id = @msgId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@msgId", msgId}
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler DeleteMessageBlocked()\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Baustelle: vor löschen prüfen, ob Benutezr in aktiver ode rzukünftiger Schicht eingeteilt ist!
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public bool DeleteContact(int contactId)
        {
            try
            {
                const string query = "DELETE FROM \"Contact\" WHERE Id = @contactId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@contactId", contactId }
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler DeleteContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public bool DeleteCompany(int companyId)
        {
            try
            {
                const string query = "DELETE FROM \"Company\" WHERE Id = @companyId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@companyId", companyId }
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler DeleteCompany() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

        public bool DeleteShift(int shiftId)
        {
            try
            {
                const string query = "DELETE FROM \"Shifts\" WHERE Id = @shiftId; ";

                Dictionary<string, object> args = new Dictionary<string, object>
                {
                    { "@shiftId", shiftId }
                };

                return SqlNonQuery(query, args);
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler DeleteShift() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }

    }
}
