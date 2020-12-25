using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    /* MelBox2
     * Diese Konsolenanwendung bündelt mehrere Klassenbibliotheken, und führt diese aus.
     * 
     */
    
    partial class Program
    {
        #region Properties
        internal static MelBoxSql Sql = new MelBoxSql();
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

                Email.PermanentEmailRecievers = MelBox2inEins.Properties.Settings.Default.Einstellung.Cast<string>().ToList();

                Gsm_Com.ComPortName = MelBox2inEins.Properties.Settings.Default.ComPort;

                Gsm_Com.GsmConnected += HandleGsmEvent;
                Gsm_Com.GsmEvent += HandleGsmEvent;

                Gsm.Connect();
                Gsm.GsmSignalQualityEvent += HandleGsmEvent;
                Gsm.SmsRecievedEvent += HandleSmsRecievedEvent;
                Gsm.SmsSentEvent += HandleSmsSentEvent;

                //  gsm.SmsSend(4916095285304, "MelBox2 gestartet.");

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
                        Gsm_Com.AddAtCommand(at);
                    }
                } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
            }
            finally
            {
                Console.WriteLine("Programm wird geschlossen...");
                Gsm_Com.ClosePort();
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}
