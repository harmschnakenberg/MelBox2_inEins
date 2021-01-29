using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MelBox2;

namespace MelBox2
{
    public static class Gsm_Basics
    {
        #region Fields
        private static SerialPort Port;
        const int maxConnectTrys = 7;
        static int currentConnectTrys = 0;
        static readonly System.Timers.Timer sendTimer = new System.Timers.Timer(1000); //2000 funktioniert
        #endregion

        #region Properties
        public static string ComPortName { get; set; } = "COM1";

        public static int BaudRate { get; set; } = 9600;

        /// <summary>
        /// Liste der anstehenden AT-Commands zur sequenziellen Abarbeitung
        /// </summary>
        private static List<string> ATCommandQueue { get; set; } = new List<string>();
        #endregion

        #region Events
        /// <summary>
        /// Event 'GSM-Ereignis'
        /// </summary>
        public static event EventHandler<GsmEventArgs> GsmEvent;

        /// <summary>
        /// Trigger für das Event 'GSM-Ereignis'
        /// </summary>
        /// <param name="e"></param>
        public static void RaiseGsmEvent(GsmEventArgs.Telegram telegram, string eventContent, object Payload = null)
        {
            GsmEvent?.Invoke(null, new GsmEventArgs(telegram, eventContent, Payload));
            //Console.WriteLine(eventContent);
        }

        public static event EventHandler<GsmEventArgs> GsmConnected;

        private static void RaiseGsmConnected(bool connected, string ComPortName)
        {
            GsmConnected?.Invoke(null, new GsmEventArgs(GsmEventArgs.Telegram.GsmConnection, ComPortName + (connected ? " verbunden":" getrennt"), connected));
        }

        #endregion

        #region Constructor

        #endregion

        #region Verbindung

        /// <summary>
        /// Nach AppStart erstmals Verbindung zum COM-Port herstellen und GSM-Modem initialisieren 
        /// </summary>
        public static void Connect()
        {
            ConnectPort();

            if (Port == null || !Port.IsOpen) //Verbindung ist fehlgeschlagen
            {
                ClosePort();
                System.Threading.Thread.Sleep(5000); //Pause zum lesen der Bildschirmausgabe.
                Environment.Exit(0);
            }

            #region Timer zum koordinierten Senden über GSM-Modem
            sendTimer.Elapsed += (sender, eventArgs) =>
            {
                if (ATCommandQueue.Count > 0)
                    SendNextATCommand();
                else
                    sendTimer.Stop();
            };
            sendTimer.AutoReset = true;
            sendTimer.Enabled = false;
            #endregion

            RaiseGsmConnected(true, Port.PortName);
        }

        /// <summary>
        /// interner Aufruf COM-Port verbinden
        /// </summary>
        private static void ConnectPort()
        {
            #region richtigen COM-Port ermitteln
            List<string> AvailableComPorts = System.IO.Ports.SerialPort.GetPortNames().ToList();

            if (AvailableComPorts.Count < 1)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Es sind keine COM-Ports vorhanden");
                return;
            }

            if (!AvailableComPorts.Contains(ComPortName))
            {
                ComPortName = AvailableComPorts.LastOrDefault();
            }
            #endregion

            #region Wenn Port bereits vebunden ist, trennen
            if (Port != null && Port.IsOpen)
            {
                ClosePort();
            }
            #endregion

            #region Verbinde ComPort
            RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, string.Format("Öffne Port {0}...", ComPortName));

            SerialPort port = new SerialPort();

            while (port == null || !port.IsOpen)
            {
                currentConnectTrys++;
                try
                {
                    port.PortName = ComPortName;                            //COM1
                    port.BaudRate = BaudRate;                               //9600
                    port.DataBits = 8;                                      //8
                    port.StopBits = StopBits.One;                           //1
                    port.Parity = Parity.None;                              //None
                    port.ReadTimeout = 300;                                 //300
                    port.WriteTimeout = 300;                                //300
                    port.Encoding = Encoding.GetEncoding("iso-8859-1");
                    port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                    port.ErrorReceived += new SerialErrorReceivedEventHandler(Port_ErrorReceived);
                    port.Open();
                    port.DtrEnable = true;
                    port.RtsEnable = true;

                    RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Verbindungsversuch " + currentConnectTrys + " von " + maxConnectTrys);

                }
                catch (ArgumentException ex_arg)
                {
                    RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("COM-Port {0} konnte nicht verbunden werden. \r\n{1}\r\n{2}", ComPortName, ex_arg.GetType(), ex_arg.Message));
                    Thread.Sleep(2000);
                }
                catch (UnauthorizedAccessException ex_unaut)
                {
                    RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("Der Zugriff auf COM-Port {0} wurde verweigert. \r\n{1}\r\n{2}", ComPortName, ex_unaut.GetType(), ex_unaut.Message));
                    Thread.Sleep(2000);
                }
                catch (System.IO.IOException ex_io)
                {
                    RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("Verbindungsversuch {0}/{1}: COM-Port {2} konnte nicht erreicht werden.\r\n{3}", currentConnectTrys, maxConnectTrys, ComPortName, ex_io.Message));
                    Thread.Sleep(2000);
                }

                if (port == null || !port.IsOpen)
                {
                    if (currentConnectTrys >= maxConnectTrys)
                    {
                        RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Maximale Anzahl Verbindungsversuche zu " + ComPortName + " überschritten.");
                        ClosePort();
                        Environment.Exit(0);
                        return;
                    }

                    Console.WriteLine("Warte 5 sec. bis zum nächsten Verbindunggsversuch...");
                    Thread.Sleep(5000);
                }
            }

            currentConnectTrys = 0;
            Port = port;
            //RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Verbindung über " + Port.PortName + " hergestellt.");
            #endregion
        }

        /// <summary>
        /// Schließt den Port und räumt auf
        /// </summary>
        public static void ClosePort()
        {
            if (Port == null) return;

            RaiseGsmConnected(false, Port.PortName);
           // RaiseGsmEvent( GsmEventArgs.Telegram.GsmSystem, "Port " + Port.PortName + " wird geschlossen.\r\n");

            try
            {
                Port.Close();
                Port.DataReceived -= new SerialDataReceivedEventHandler(Port_DataReceived);
                Port.Dispose();
                Port = null;
                System.Threading.Thread.Sleep(3000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

        #region GSM Daten senden



        /// <summary>
        /// AT-Befehl zur Abarbeitung einreihen
        /// </summary>
        /// <param name="command">AT-Befehl</param>
        public static void AddAtCommand(string command)
        {
            if(!ATCommandQueue.Contains(command))
                ATCommandQueue.Add(command); //Stellt die Abarbeitung nacheinander sicher

            sendTimer.Start();
        }

        /// <summary>
        /// Abarbeiten der anstehenden AT-Befehle
        /// </summary>
        internal static void SendNextATCommand()
        {
            if (Port == null || !Port.IsOpen)
            {
                ConnectPort();
                Thread.Sleep(2000);
            }

            try
            {
                if (ATCommandQueue.Count > 0) //Abarbeitung nacheinander
                {
                    if (Port != null)
                    {
                        string command = ATCommandQueue.FirstOrDefault();
                      
                        Port.Write(command + "\r");
                        ATCommandQueue.Remove(command);
                        RaiseGsmEvent(GsmEventArgs.Telegram.GsmSent, command);
                        //hier keine Pause! sonst Antwortempfang unvollständig!
                    }
                }
            }
            catch (System.IO.IOException ex_io)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, ex_io.Message);
            }
            catch (InvalidOperationException ex_inval)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, ex_inval.Message);
            }
            catch (UnauthorizedAccessException ex_unaut)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, ex_unaut.Message);
            }
        }

        #endregion

        #region GSM Daten empfangen

        internal static void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            //Console.WriteLine("Fehler von COM-Port: " + e.EventType);
            RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Fehler von COM-Port: " + e.EventType);
            //ClosePort(); Böse!
        }

        internal static void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //PermissionToSend = true;

            if (Port == null || !Port.IsOpen)
            {
                return;
            }

            string answer = ReadFromPort();

            if (answer.Contains("ERROR"))
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, "Fehlerhaft Empfangen:\n\r" + answer);
            }
            else if (answer.Length > 1)
            {
                //Send data to whom ever interested
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmRec, answer);
            }

           // PermissionToSend = true;
        }

        /// <summary>
        /// Der eigentliche Lesevorgang von Port
        /// </summary>
        /// <returns></returns>
        private static string ReadFromPort()
        {
            try
            {
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
                string answer = string.Empty;
                while (answer.Length < 2)
                {
                    System.Threading.Thread.Sleep(Port.ReadTimeout); //Ist sonst unvollständig
                    answer += Port.ReadExisting();
                }
                return answer;
            }
            catch (TimeoutException ex_time)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("Der Port {0} konnte nicht erreicht werden. Timeout. \r\n{1}\r\n{2}", Port.PortName, ex_time.GetType(), ex_time.Message));
                return string.Empty;
            }
            catch (InvalidOperationException ex_op)
            {
                RaiseGsmEvent(GsmEventArgs.Telegram.GsmError, string.Format("Der Port {0} ist geschlossen \r\n{1}", Port.PortName, ex_op.Message));
                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.GetType().Name + "\r\n" + ex.Message);
            }
        }

        #endregion
    }
}
