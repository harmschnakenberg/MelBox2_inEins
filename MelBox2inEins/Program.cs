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
        internal static MelBoxSql Sql = new MelBoxSql();

        #endregion

        #region Properties
  
        #endregion

        //internal static Gsm gsm = new Gsm();
        private static void Main()
        {
            const string help = "- ENTF zum Aufräumen der Anzeige\r\n" +
                                "- EINF für AT-Befehl\r\n" +
                                "- ESC Taste zum beenden...";
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

                Sql.Log(MelBoxSql.LogTopic.Start, MelBoxSql.LogPrio.Info, string.Format( "MelBox2 - Anwendung gestartet. {0}, BaudRate {1}", Gsm_Basics.ComPortName, Gsm_Basics.BaudRate) );
                InitDailyCheck();

                //TEST
                Sql.CheckDbBackup();

                Gsm.Connect();

                #if DEBUG
                    Console.WriteLine("\r\nDEBUG Mode: es wird keine StartUp-Info an MelBox2-Admin gesendet.");
                #else
                    Gsm.SmsSend(Properties.Settings.Default.MelBoxAdminPhone, "MelBox2 - Anwendung neu gestartet um " + DateTime.Now); //Email?
                #endif

                Console.WriteLine(help);
                do
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Delete)
                    {                        
                        Console.Clear();
                        Console.WriteLine(help);
                        GlobalProperty.ShowOnConsole();
                    }

                    if (Console.ReadKey(true).Key == ConsoleKey.Insert)
                    {
                        Console.WriteLine("AT-Befehl eingeben:");
                        string at = Console.ReadLine();
                        Gsm_Basics.AddAtCommand(at);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            finally
            {
                Console.WriteLine("Programm wird geschlossen...");
                Gsm_Basics.ClosePort();
                System.Threading.Thread.Sleep(3000);
            }
        }

       


    }
}
