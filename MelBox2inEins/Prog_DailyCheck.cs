using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MelBox2
{

	partial class Program
	{
		#region Tägliche Überprüfung
		private static Timer tDaily;

		/// <summary>
		/// Startet einen Timer, der morgen um 8 Uhr eine Überprüfung startet.
		/// </summary>
		private static void InitDailyCheck()
		{
			Console.WriteLine("Starte Timer\t'tägliche Überprüfungen'");
			double MilliSecTo8am = (DateTime.Now.Date.AddDays(1).AddHours(8) - DateTime.Now).TotalMilliseconds; //Morgen um 8 Uhr

			tDaily = new Timer(MilliSecTo8am);
			tDaily.Elapsed += new ElapsedEventHandler(DailyCheck);
			tDaily.AutoReset = false;
			tDaily.Start();
		}

		/// <summary>
		/// Soll einmal täglich ausgeführt werden für Inaktivitätsprüfung, DB-Backup
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void DailyCheck(object sender, ElapsedEventArgs e)
		{
			//Timer stoppen, wird am Ende der Methode neu gestartet.
			tDaily.Stop();
			tDaily.Dispose();

			//Lebensbit-Meldung an MelBox2-Admin
#if DEBUG
			Console.WriteLine("\r\nDEBUG-Mode: Es wird keine Routinemeldung (Lebens-Bit) an MelBox2-Admin versendet.");
#else
			Gsm.SmsSend(Properties.Settings.Default.MelBoxAdminPhone, "MelBox2 - Tägliche Routinemeldung");
#endif
			// Prüfe Inaktivität Sender
			InactiveNotification();

			//Prüfe Backup Datenbank
			Sql.CheckDbBackup();

			// Timer neu starten, um TimeSpan bis morgen 8 Uhr neu zu berechnen
			InitDailyCheck();
		}
		#endregion

		/// <summary>
		/// BAUSTELLE: nicht getestet
		/// </summary>
		static void InactiveNotification()
		{
			DataTable dt = Sql.GetViewMsgOverdue();

			foreach (DataRow row in dt.Rows)
			{
				// "CREATE VIEW \"ViewMessagesOverdue\" AS SELECT FromContactId AS Id, Name, MaxInactiveHours || ' Std.' AS Max_Inaktiv, RecieveTime AS Letzte_Nachricht, " +
				// "CAST( (strftime('%s','now') - strftime('%s',RecieveTime, '+' || MaxInactiveHours ||' hours'))/3600 AS INTEGER) || ' Std.' AS Fällig_seit FROM LogRecieved " +
				// "JOIN Contact ON Contact.Id = LogRecieved.FromContactId WHERE MaxInactiveHours > 0 AND DATETIME(RecieveTime, '+' || MaxInactiveHours ||' hours') < Datetime('now'); " +

				string col2 = row["Name"].ToString();
				//string col3 = row["Max_Inactive"].ToString();
				string col4 = row["Letzte_Nachricht"].ToString();
				string col5 = row["Fällig_seit"].ToString(); ;

				string msg = string.Format("Zeitüberschreitung {0} für {1} - letzte Nachricht {2}", col5, col2, col4);

                //ulong phone = Properties.Settings.Default.MelBoxAdminPhone;
                //Gsm.SmsSend(phone, msg);

                Email.Send(Email.SMSCenter, msg);				
			}
		}

	}
}
