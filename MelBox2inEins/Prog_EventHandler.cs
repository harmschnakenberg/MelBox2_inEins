using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    partial class Program
    {
		static void HandleGsmEvent(object sender, GsmEventArgs e)
		{
			switch (e.Type)
			{
				case GsmEventArgs.Telegram.GsmConnection:
					GlobalProperty.ConnectedToModem = (bool)e.Payload;
					break;
				case GsmEventArgs.Telegram.GsmError:
					Console.ForegroundColor = ConsoleColor.Red;
					Sql.Log(MelBoxSql.LogTopic.Sms, MelBoxSql.LogPrio.Error, e.Message);
					break;
				case GsmEventArgs.Telegram.GsmSystem:
					Console.ForegroundColor = ConsoleColor.Gray;
					break;
				case GsmEventArgs.Telegram.GsmOwnPhone:
					GlobalProperty.OwnPhone = (ulong)e.Payload;
					return;//keine erneute Ausgabe in Console
				case GsmEventArgs.Telegram.GsmSignal:
					Console.ForegroundColor = ConsoleColor.Yellow;
					GlobalProperty.GsmSignalQuality = (int)e.Payload;
					break;
				case GsmEventArgs.Telegram.GsmRec:
					Console.ForegroundColor = ConsoleColor.DarkGreen;
					break;
				case GsmEventArgs.Telegram.GsmSent:
					Console.ForegroundColor = ConsoleColor.DarkYellow; 
					break;
				case GsmEventArgs.Telegram.SmsRec:
					Console.ForegroundColor = ConsoleColor.Cyan;
					Sql.Log(MelBoxSql.LogTopic.Sms, MelBoxSql.LogPrio.Info, "Empfangen:" + e.Message);
					break;
				case GsmEventArgs.Telegram.SmsStatus:
					Console.ForegroundColor = ConsoleColor.DarkCyan;
					Sql.Log(MelBoxSql.LogTopic.Sms, MelBoxSql.LogPrio.Info, "Status: " + e.Message);
					break;
				case GsmEventArgs.Telegram.SmsSent:
					Console.ForegroundColor = ConsoleColor.DarkBlue;
					Sql.Log(MelBoxSql.LogTopic.Sms, MelBoxSql.LogPrio.Info, "Gesendet: " + e.Message);
					break;
				default:
					Console.ForegroundColor = ConsoleColor.White;
					break;
			}

			Console.WriteLine(e.Type.ToString() + ":\t" + e.Message);
			Console.ForegroundColor = ConsoleColor.Gray;

		}

		static void HandleSmsRecievedEvent(object sender, Sms e)
		{
			//Neue SMS-Nachricht empfangen
			Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.SmsRec, e.Message);

			//Neue Nachricht in DB speichern
			Sql.InsertMessageRec(e.Message, e.Phone);

			if (e.Message.ToLower().StartsWith(GlobalProperty.SmsRouteTestTrigger.ToLower()))
			{
				//SMS mit 'MeldeAbruf' Empfangen
				Gsm.SmsSend(e.Phone, e.Message + " um " + DateTime.Now);
			}
			else
			{
				//Für jeden Empfänger (Bereitschaft) eine SMS vorbereiten
				foreach (ulong phone in Sql.GetCurrentShiftPhoneNumbers())
				{
					//Nachricht per SMS weiterleiten
					Gsm.SmsSend(phone, e.Message);
				}

				//Nachricht per email weiterleiten
				Email.Send(Sql.GetCurrentShiftEmail(), e.Message);
			}


			Gsm.ReadGsmMemory();
		}


		static void HandleSmsSentEvent(object sender, Sms e)
		{
			//Neue bzw. erneut SMS-Nachricht versendet
			//Neue SMS-Nachricht empfangen
			Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.SmsSent, e.Message);

			Sql.InsertMessageSent(e.Message, e.Phone, e.MessageReference);
		}

		static void HandleSmsStatusReportEvent(object sender, Sms e)
		{
			//Empfangener Statusreport (z.B. Sendebestätigung)
			Gsm_Com.RaiseGsmEvent(GsmEventArgs.Telegram.SmsStatus, string.Format("Empfangsbestätigung für SMS-Referrenz {0}", e.MessageReference) );

			Sql.InsertMessageStatus(e.MessageReference, e.SendStatus);
			Sql.UpdateMessageSentStatus(MelBoxSql.SendWay.Sms, e.MessageReference, e.SmsProviderTimeStamp, e.SendStatus);
		}

	}

	public static class GlobalProperty
    {
		
		public static bool ConnectedToModem { get; set; } = false;

		public static bool SimDetected { get; set; } = false;

		public static ulong OwnPhone { get; set; }

		public static string NetworkRegistrationStatus { get; set; } = "noch nicht erfasst";

		public static int GsmSignalQuality { get; set; } = 0;

		public static int SmsPendingReports { get; set; } = 0;

		public static string SmsRouteTestTrigger { get; set; } = "SMSAbruf";

		public static string LastSmsSend { get; set; } = "-keine-";

		public static void ShowOnConsole()
        {
			const int tabPos = 32;
			Console.WriteLine("PROGRAMM - STATUS");

			Console.Write("Verbunden mit GSM-Modem");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine(ConnectedToModem ? "verbunden" : "keine Verbindung");

			Console.Write("SIM erkannt");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine(SimDetected ? "erkannt" : "nicht erkannt");

			Console.Write("Eigene Telefonnummer");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine("+" + OwnPhone);

			Console.Write("Registrierung Mobilfunknetz");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine(NetworkRegistrationStatus);

			Console.Write("Mobilfunknetz Empfangsqualität");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine("{0:0}%", GsmSignalQuality);

			Console.Write("Fehlende Empfangsbestätigungen");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine("{0}", SmsPendingReports);

			Console.Write("SMS-Text Meldeweg Test");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine(SmsRouteTestTrigger);

			Console.Write("Zuletzt gesendete SMS");
			Console.SetCursorPosition(tabPos, Console.CursorTop);
			Console.WriteLine(LastSmsSend);
		}
    }

}
