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
			Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.SmsRec, e.Message);

			//Neue Nachricht in DB speichern
			Sql.InsertMessageRec(e.Message, e.Phone);

			if (e.Message.ToLower().StartsWith(GlobalProperty.SmsRouteTestTrigger.ToLower()))
			{
				//SMS mit 'MeldeAbruf' Empfangen
				Gsm.SmsSend(e.Phone, e.Message + " um " + DateTime.Now);
			}
			else
			{
				if (Sql.IsMessageBlockedNow(e.Message))
				{
					Sql.Log(MelBoxSql.LogTopic.Shift, MelBoxSql.LogPrio.Info, "Keine SMS: " + e.Message);
				}
                else 
				{ 
					//Für jeden Empfänger (Bereitschaft) eine SMS vorbereiten
					foreach (ulong phone in Sql.GetCurrentShiftPhoneNumbers())
					{
						//Nachricht per SMS weiterleiten
						Gsm.SmsSend(phone, e.Message);
					}
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
			Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.SmsSent, e.Message);

			Sql.InsertMessageSent(e.Message, e.Phone, e.MessageReference);
		}

		static void HandleSmsStatusReportEvent(object sender, Sms e)
		{
			//Empfangener Statusreport (z.B. Sendebestätigung)
			Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.SmsStatus, string.Format("Empfangsbestätigung für SMS-Referrenz {0}", e.MessageReference) );

			Sql.InsertMessageStatus(e.MessageReference, e.SendStatus);
			Sql.UpdateMessageSentStatus(MelBoxSql.SendWay.Sms, e.MessageReference, e.SmsProviderTimeStamp, e.SendStatus);
		}

	}


}
