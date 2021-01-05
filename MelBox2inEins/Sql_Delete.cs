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

        public void DeleteMessageBlocked(int msgId)
        {           
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = "DELETE FROM BlockedMessages WHERE Id = @msgId; ";
                                         
                    command.Parameters.AddWithValue("@msgId", msgId);

                    command.ExecuteNonQuery();
                }
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
        public int DeleteContact(int contactId)
        {
            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = "DELETE FROM \"Contact\" WHERE Id = @contactId; ";
                    command.Parameters.AddWithValue("@contactId", contactId);

                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler DeleteContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }


    }
}
