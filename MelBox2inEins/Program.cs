using System;
using System.Linq;
using System.Timers;

namespace MelBox2
{
    /* MelBox2
     * besteht aus drei Teilen:
     * - GSM-Modem "Treiber"
     * - Datenbankverwaltung
     * - Webinterface
     * 
     * Zeiten: In der Anwendung wird mit lokaler Zeit gerechent, in der Datenbank werden UTC-Zeiten gespeichert.
     */

    partial class Program
    {
        #region Fields
        public static MelBoxSql Sql = new MelBoxSql();

        #endregion

        #region Properties
  
        #endregion

        private static void Main()
        {
            try
            {                
                Console.BufferHeight = 1000; //Max. Zeilen in Konsole, bei Überlauf werden älteste Zeilen entfernt
                Console.WriteLine("Programm gestartet. Konsole mit max. {0} Zeilen.", Console.BufferHeight);

                Email.PermanentEmailRecievers = Properties.Settings.Default.PermanentEmailRecievers.Cast<string>().ToList();
                Email.MelBox2Admin = new System.Net.Mail.MailAddress( "harm.schnakenberg@kreutztraeger.de", "MelBox2 Admin"); //Properties gehen nicht?

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
                
                InitDailyCheck( Properties.Settings.Default.HourOfDailyCheck ); //Führt täglich morgens um 8 Uhr Routinecheck aus

                //TEST
                Sql.InsertMessageRec("Testnachricht am " + DateTime.Now.Date , 4915142265412);

                //Auskommentiert für Test WebServer
                Gsm.Connect();

#if DEBUG
                Console.WriteLine("\r\nDEBUG Mode: es wird keine StartUp-Info an MelBox2-Admin gesendet.");
                //Email.Send(Email.MelBox2Admin, "MelBox2 - Anwendung neu gestartet um " + DateTime.Now);
#else
                //Gsm.SmsSend(Properties.Settings.Default.MelBoxAdminPhone, "MelBox2 - Anwendung neu gestartet um " + DateTime.Now); //besser Email
                Email.Send(Email.MelBox2Admin, "MelBox2 - Anwendung neu gestartet um " + DateTime.Now);                    
#endif

                const string help = "\r\n- ENTF zum Aufräumen der Anzeige\r\n" +
                    "- EINF für AT-Befehl\r\n" +
                    "- ESC Taste zum beenden...\r\n";

                Console.WriteLine(help);

                while (true)
                {
                    ConsoleKeyInfo pressed = Console.ReadKey();
                    //Console.WriteLine("Gedrückt: " + pressed.KeyChar);

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

                    //if (pressed.Key == ConsoleKey.O)
                    //{
                    //    Console.Clear();
                    //    GlobalProperty.ShowOnConsole();
                    //    //Console.WriteLine("\r\nOPTIONEN:\r\n\tModem: Umschalten eingehend");
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmError + ":\t" + GsmEventArgs.Telegram.GsmError);
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmSystem + ":\t" + GsmEventArgs.Telegram.GsmSystem);
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmConnection + ":\t" + GsmEventArgs.Telegram.GsmConnection);
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmSignal + ":\t" + GsmEventArgs.Telegram.GsmSignal);
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmOwnPhone + ":\t" + GsmEventArgs.Telegram.GsmOwnPhone);
                    //    //Console.WriteLine((byte)GsmEventArgs.Telegram.GsmOwnPhone + ":\t" + GsmEventArgs.Telegram.GsmOwnPhone);

                    //    //Console.WriteLine("Konsolenanzeige: Bitweise Filter setzten (sperren):");
                    //    //Console.WriteLine("\r\n\tBitte Nummer auswählen.");

                    //    //string input = Console.ReadLine();
                    //    //if (byte.TryParse(input, out byte a))
                    //    //{
                    //    //    ConsoleDisplayBlock = a;
                    //    //}

                    //    Console.Clear();
                    //    Console.WriteLine(help);
                    //}

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
