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

    }
}
