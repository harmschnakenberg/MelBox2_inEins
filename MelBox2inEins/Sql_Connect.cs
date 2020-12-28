﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace MelBox2
{
    public partial class MelBoxSql
    {

        private readonly string DataSource = "Data Source=" + DbPath;
        internal static string DbPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DB", "MelBox2.db");

        #region Methods

        public MelBoxSql(string dbPath = "")
        {
            if (dbPath.Length > 0)
            {
                DbPath = dbPath;
            }

            #region Prüfe Datenbank-Datei 
            //Datenbak prüfen / erstellen
            if (!System.IO.File.Exists(DbPath))
            {
                CreateNewDataBase();
            }

            FileInfo dbFileInfo = new FileInfo(DbPath);

            if (IsFileLocked(dbFileInfo))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\r\n\r\n*** ZUGRIFFSFEHLER ***\r\n\r\n" +
                    "Die Datenbankdatei\r\n" + DbPath +
                    "\r\nist durch ein anderes Programm blockiert.\r\n\r\n" +
                    "Das Programm wird beendet\r\n\r\n" +
                    "*** PROGRAMM WIRD BEENDET***");
                Thread.Sleep(10000);
                Environment.Exit(0);
            }
            #endregion
        }
        /// <summary>
        /// Erzeugt eine neue Datenbankdatei, erzeugt darin Tabellen, Füllt diverse Tabellen mit Defaultwerten.
        /// </summary>
        private void CreateNewDataBase()
        {
            Console.WriteLine("Erstelle eine neue Datenbank.");

            //Erstelle Datenbank-Datei und öffne einmal
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
            FileStream stream = File.Create(DbPath);
            stream.Close();

            //Erzeuge Tabellen in neuer Datenbank-Datei
            //Zeiten im Format TEXT (Lesbarkeit Rohdaten)
            using (var connection = new SqliteConnection(DataSource))
            {
                SQLitePCL.Batteries.Init();
                //SQLitePCL.raw.SetProvider(new  );

                connection.Open();

                const string query = "CREATE TABLE \"Log\"(\"Id\" INTEGER NOT NULL PRIMARY KEY UNIQUE,\"LogTime\" TEXT NOT NULL, \"Topic\" TEXT , \"Prio\" INTEGER NOT NULL, \"Content\" TEXT); " +
                //Kontakte
                "CREATE TABLE \"Company\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"Name\" TEXT NOT NULL, \"Address\" TEXT, \"City\" TEXT); " +

                "CREATE TABLE \"Contact\"(\"Id\" INTEGER PRIMARY KEY AUTOINCREMENT UNIQUE, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"Name\" TEXT NOT NULL, \"Password\" TEXT, " +
                "\"CompanyId\" INTEGER, \"Email\" TEXT, \"Phone\" INTEGER, \"KeyWord\" TEXT, \"MaxInactiveHours\" INTEGER DEFAULT 0, \"SendSms\" INTEGER NOT NULL CHECK( \"SendSms\" < 2 ), \"SendEmail\" INTEGER NOT NULL CHECK( \"SendEmail\" < 2 ) ); " +

                //Nachrichten
                "CREATE TABLE \"MessageContent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"Content\" TEXT NOT NULL UNIQUE );" +

                "CREATE TABLE \"LogRecieved\"( \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"RecieveTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"FromContactId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL); " +

                "CREATE TABLE \"LogSent\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"SentToId\" INTEGER NOT NULL, \"ContentId\" INTEGER NOT NULL, \"SentVia\" INTEGER NOT NULL, \"SmsRef\" INTEGER, \"ConfirmStatus\" INTEGER); " +

                "CREATE TABLE \"LogStatus\" (\"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"SentTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"SentVia\" INTEGER NOT NULL, \"SmsRef\" INTEGER, \"ConfirmStatus\" INTEGER); " +

                //Bereitschaft
                "CREATE TABLE \"Shifts\"( \"Id\" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, " +
                "\"ContactId\" INTEGER NOT NULL, \"StartTime\" TEXT NOT NULL, \"EndTime\" TEXT NOT NULL ); " +

                "CREATE TABLE \"BlockedMessages\"( \"Id\" INTEGER NOT NULL UNIQUE, \"EntryTime\" TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP, \"StartHour\" INTEGER NOT NULL, " +
                "\"EndHour\" INTEGER NOT NULL, \"Days\" INTEGER NOT NULL); " +

                //Hilfstabelle

                //Views
                "CREATE VIEW \"ViewMessagesRecieved\" AS SELECT r.Id As Nr, RecieveTime AS Empfangen, c.Name AS von, Content AS Inhalt " +
                "FROM LogRecieved AS r JOIN Contact AS c ON FromContactId = c.Id JOIN MessageContent AS m ON ContentId = m.Id; " +

                "CREATE VIEW \"ViewMessagesSent\" AS SELECT SentTime AS Gesendet, c.name AS An, Content AS Inhalt, SentVia AS Via, ConfirmStatus AS Sendestatus " +
                "FROM LogSent AS ls JOIN Contact AS c ON SentToId = c.Id JOIN MessageContent AS mc ON mc.id = ls.ContentId; " +

                "CREATE VIEW \"ViewMessagesOverdue\" AS SELECT FromContactId AS Id, cont.Name, comp.Name AS Firma, MaxInactiveHours || ' Std.' AS Max_Inaktiv, RecieveTime AS Letzte_Nachricht, CAST( (strftime('%s', 'now') - strftime('%s', RecieveTime, '+' || MaxInactiveHours || ' hours')) / 3600 AS INTEGER) || ' Std.' AS Fällig_seit " +
                "FROM LogRecieved JOIN Contact AS cont ON cont.Id = LogRecieved.FromContactId JOIN Company AS comp ON comp.Id = cont.CompanyId WHERE MaxInactiveHours > 0 AND DATETIME(RecieveTime, '+' || MaxInactiveHours || ' hours') < Datetime('now'); " +

                "CREATE VIEW \"ViewMessagesBlocked\" AS SELECT BlockedMessages.Id AS Id, Content As Nachricht, StartHour || ' Uhr' As Beginn, EndHour || ' Uhr' As Ende, " +
                "(SELECT Days & 1 > 0) AS So, (SELECT Days & 2 > 0) AS Mo, (SELECT Days & 3 > 0) AS Di, (SELECT Days & 4 > 0) AS Mi, (SELECT Days & 5 > 0) AS Do, (SELECT Days & 6 > 0) AS Fr, (SELECT Days & 7 > 0) AS Sa " +
                " FROM BlockedMessages JOIN MessageContent ON MessageContent.Id = BlockedMessages.Id; ";

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();

                Log(LogTopic.Start, LogPrio.Info, "Datenbank neu erstellt.");

                InsertCompany("_UNBEKANNT_", "Musterstraße 123", "12345 Modellstadt");
                InsertCompany("Kreutzträger Kältetechnik GmbH & Co. KG", "Theodor-Barth-Str. 21", "28307 Bremen");

                //\"Id\", \"EntryTime\", \"Name\", \"Password\", \"CompanyId\", \"Email\", \"Phone\", \"KeyWord\", \"MaxInactiveHours\", \"SendSms\", \"SendEmail\"
                InsertContact("SMSZentrale", "7307", 2, "smszentrale@kreutztraeger.de", 4915142265412, 0, false, false);
                InsertContact("MelBox2Admin", null, 2, "harm.schnakenberg@kreutztraeger.de", 0, 0, false, true);
                InsertContact("Bereitschaftshandy", null, 2, "bereitschaftshandy@kreutztraeger.de", 491728362586, 0, false, false);
                InsertContact("Kreutzträger Service", null, 2, "service@kreutztraeger.de", 0, 0, false, false);
                InsertContact("Henry Kreutzträger", null, 2, "henry.kreutztraeger@kreutztraeger.de", 491727889419, 0, false, false);
                InsertContact("Bernd Kreutzträger", null, 2, "bernd.kreutztraeger@kreutztraeger.de", 491727875067, 0, false, false);
                InsertContact("Harm privat", null, 1, "harm.schnakenberg@kreutztraeger.de", 4916095285304, 0, true, false);

                InsertMessageRec("Datenbank neu erstellt.", 0, "smszentrale@kreutztraeger.de");

                InsertMessageSent(1, 1, SendWay.Unknown, 0, SendStatus.OnlyDb);

                //Dummy
                InsertShift(7, DateTime.Now.AddHours(-1), DateTime.Now.AddHours(14));
                InsertShift(7, DateTime.Now.AddDays(1).AddHours(-1), DateTime.Now.AddDays(1).AddHours(14));
                InsertShift(7, DateTime.Now.AddDays(2).AddHours(-1), DateTime.Now.AddDays(2).AddHours(14));
                InsertShift(7, DateTime.Now.AddDays(3).AddHours(-1), DateTime.Now.AddDays(3).AddHours(14));
                InsertShift(7, DateTime.Now.AddDays(4).AddHours(-1), DateTime.Now.AddDays(4).AddHours(14));

                InsertMessageBlocked(1, 7);
            }

        }

        /// <summary>
        /// Erstellt ein wöchentliches Backup der Datenbank, wenn dies nicht vorhanden ist.
        /// </summary>
        public void CheckDbBackup()
        {
            try
            {
                string backupPath = Path.Combine(Path.GetDirectoryName(DbPath), string.Format("MelBox2_{0}_KW{1:00}.db", DateTime.UtcNow.Year, GetIso8601WeekOfYear(DateTime.UtcNow) ) );
                if (File.Exists(backupPath)) return;

                using (var connection = new SqliteConnection(DataSource))
                {
                    connection.Open();

                    // Create a full backup of the database
                    var backup = new SqliteConnection("Data Source=" + backupPath);
                    connection.BackupDatabase(backup);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Sql-Fehler CheckDbBackup()\r\n" + ex.Message);
            }
        }
        #endregion
    }
}