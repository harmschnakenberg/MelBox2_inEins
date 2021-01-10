using System;
using System.Linq;
using System.Timers;

namespace MelBox2
{
    /* MelBox2
     * Diese Konsolenanwendung bündelt mehrere Klassenbibliotheken, und führt diese aus.
     * 
     */

    partial class Program
    {
        #region Fields
        public static MelBoxSql Sql = new MelBoxSql();

        #endregion

        #region Properties
  
        #endregion

        //internal static Gsm gsm = new Gsm();
        private static void Main()
        {

            try
            {
                Console.WriteLine("Programm gestartet.");
                Console.BufferHeight = 1000; //Max. Zeilen in Konsole, bei Überlauf werden älteste Zeilen entfernt

                Email.PermanentEmailRecievers = Properties.Settings.Default.PermanentEmailRecievers.Cast<string>().ToList();

                Gsm_Basics.ComPortName = Properties.Settings.Default.ComPort;
                Gsm_Basics.BaudRate = Properties.Settings.Default.BaudRate;
                Gsm_Basics.GsmConnected += HandleGsmEvent;
                Gsm_Basics.GsmEvent += HandleGsmEvent;

                Gsm.GsmSignalQualityEvent += HandleGsmEvent;
                Gsm.SmsRecievedEvent += HandleSmsRecievedEvent;
                Gsm.SmsSentEvent += HandleSmsSentEvent;
                Gsm.SmsStatusreportEvent += HandleSmsStatusReportEvent;

                MelBoxWeb.StartWebServer();

                Sql.Log(MelBoxSql.LogTopic.Start, MelBoxSql.LogPrio.Info, string.Format( "MelBox2 - Anwendung gestartet. {0}, {1} Baud", Gsm_Basics.ComPortName, Gsm_Basics.BaudRate) );
                InitDailyCheck();

                //TEST
                Sql.InsertMessageRec("Testnachricht am " + DateTime.Now.Date , 4916095285304);

                //Auskommentiert für Test WebServer
           //     Gsm.Connect();

#if DEBUG
                    Console.WriteLine("\r\nDEBUG Mode: es wird keine StartUp-Info an MelBox2-Admin gesendet.");
#else
                    Gsm.SmsSend(Properties.Settings.Default.MelBoxAdminPhone, "MelBox2 - Anwendung neu gestartet um " + DateTime.Now); //Email?
#endif

                const string help = "\r\n- ENTF zum Aufräumen der Anzeige\r\n" +
                    "- EINF für AT-Befehl\r\n" +
                    "- ESC Taste zum beenden...\r\n";

                Console.WriteLine(help);

                while (true)
                {
                    ConsoleKeyInfo pressed = Console.ReadKey();

                    if (pressed.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    if (pressed.Key == ConsoleKey.Delete)
                    {                        
                        Console.Clear();
                        GlobalProperty.ShowOnConsole();
                        Console.WriteLine(help);
                    }

                    if (pressed.Key == ConsoleKey.Insert)
                    {
                        Console.WriteLine("AT-Befehl eingeben:");
                        string at = Console.ReadLine();
                        Gsm_Basics.AddAtCommand(at);
                    }
                }

            }
            finally
            {
                Console.WriteLine("Programm wird geschlossen...");
                MelBoxWeb.StopWebServer();
                Gsm_Basics.ClosePort();
                System.Threading.Thread.Sleep(3000);
            }
        }

       


    }
}
