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

        /// <summary>
        /// BAUSTELLE funktioniert noch nicht
        /// </summary>
        /// <param name="sendVia"></param>
        /// <param name="smsReference"></param>
        /// <param name="sendTime"></param>
        /// <param name="smsSendStatus"></param>
        public void UpdateMessageSentStatus(SendWay sendVia, int smsReference, DateTime sendTime, byte smsSendStatus = 255)
        {
            // "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
            //\"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
            //\"SentToId\", \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, 
            //\"SmsIndex\" INTEGER, \"ConfirmStatus\" INTEGER);" +

            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE \"LogSent\" " +
                          "SET \"ConfirmStatus\" = @confirmStatus " +
                          "WHERE ID = (SELECT ID FROM \"LogSent\" " +
                          "WHERE \"SmsRef\" = @smsRef " +
                          "AND \"SendVia\" = @sendVia " +
                          "AND \"ConfirmStatus\" = @confirmStatusOld " +
                          "AND \"SentTime\" BETWEEN datetime(@sentTime, '-5 minutes') AND datetime(@sentTime)" +
                          "ORDER BY \"SentTime\" DESC LIMIT 1); ";

                    //command.Parameters.AddWithValue("@sentToId", sentToId);
                    //command.Parameters.AddWithValue("@contentId", contentId);
                    command.Parameters.AddWithValue("@sendVia", sendVia);
                    command.Parameters.AddWithValue("@smsRef", smsReference);
                    command.Parameters.AddWithValue("@confirmStatus", sendStatus );
                    command.Parameters.AddWithValue("@confirmStatusOld", SendStatus.OnlyDb);
                    command.Parameters.AddWithValue("@sentTime", MelBoxSql.SqlTime(sendTime) );

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateMessageSent() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }
    }
}
