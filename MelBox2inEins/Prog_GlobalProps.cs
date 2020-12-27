using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MelBox2
{

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
