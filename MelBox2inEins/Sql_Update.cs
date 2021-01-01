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

        /// <summary>
        /// BAUSTELLE
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="companyId"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="keyWord"></param>
        public int UpdateContact(int contactId, string name = "", string password = "", int companyId = 0, string email = "", ulong phone = 0, string keyWord = "")
        {
            StringBuilder builder = new StringBuilder();

            try
            {
                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.Parameters.AddWithValue("@contactId", contactId);

                    if (name.Length > 3)
                    {
                        command.Parameters.AddWithValue("@name", name);
                        builder.Append("UPDATE \"Contact\" SET Name = @name WHERE Id = @contactId; ");
                    }

                    if (password.Length > 3)
                    {
                        command.Parameters.AddWithValue("@password", Encrypt(password));
                        builder.Append("UPDATE \"Contact\" SET Password = @password WHERE Id = @contactId; ");
                    }

                    if (companyId > 0)
                    {
                        command.Parameters.AddWithValue("@companyId", companyId);
                        builder.Append("UPDATE \"Contact\" SET CompanyId = @companyId WHERE Id = @contactId; ");
                    }

                    if (phone > 0)
                    {
                        command.Parameters.AddWithValue("@phone", phone);
                        builder.Append("UPDATE \"Contact\" SET Phone = @phone WHERE Id = @contactId; ");
                    }

                    if (IsEmail(email))
                    {
                        command.Parameters.AddWithValue("@email", email);
                        builder.Append("UPDATE \"Contact\" SET Email = @email WHERE Id = @contactId; ");
                    }

                    if (keyWord.Length > 3)
                    {
                        command.Parameters.AddWithValue("@keyWord", keyWord);
                        builder.Append("UPDATE \"Contact\" SET KeyWord = @keyWord WHERE Id = @contactId; ");
                    }

                    command.CommandText = builder.ToString();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler UpdateContact() " + ex.GetType() + "\r\n" + ex.Message);
            }
        }


    }
}
