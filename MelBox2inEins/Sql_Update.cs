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

        public void UpdateMessageSent(int contentId, int sentToId, SendWay sendVia, int smsReference, SendStatus confirmStatus = 0)
        {
            // "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                                        //\"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, 
                                        //\"SentToId\", \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, 
                                        //\"SmsIndex\" INTEGER, \"ConfirmStatus\" INTEGER);" +

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
                          "AND \"ContentId\" = @contentId " +
                          "AND \"SentToId\" = @sentToId " +
                          "AND \"SendVia\" = @sendVia " +
                          "AND \"ConfirmStatus\" = @confirmStatusOld" +
                          "ORDER BY \"SentTime\" DESC LIMIT 1; ";

                    command.Parameters.AddWithValue("@sentToId", sentToId);
                    command.Parameters.AddWithValue("@contentId", contentId);
                    command.Parameters.AddWithValue("@sendVia", sendVia);
                    command.Parameters.AddWithValue("@smsRef", smsReference);
                    command.Parameters.AddWithValue("@confirmStatus", confirmStatus);
                    command.Parameters.AddWithValue("@confirmStatus", confirmStatus);
                    command.Parameters.AddWithValue("@confirmStatusOld", SendStatus.OnlyDb);

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw new Exception("Sql-Fehler UpdateLogSent()");
            }
        }


        public void UpdateMessageSent(string message, ulong phone, byte smsRef, int smsSendStatus) 
        {
            int contentId = GetMessageId(message);
            int contactId = GetContactId("", phone, "", message);

            SendStatus sendStatus = SendStatus.SetToSent;

            if (smsSendStatus < 32)
                sendStatus = SendStatus.SentSuccess;
            else if (smsSendStatus < 64)
                sendStatus = SendStatus.Pending;
            else if (smsSendStatus < 128)
                sendStatus = SendStatus.SendAbborted;

            UpdateMessageSent(contentId, contactId, SendWay.Sms, smsRef, sendStatus);
        }

    }
}
