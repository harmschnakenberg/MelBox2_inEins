using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class Gsm
    {
        #region Fields

        #endregion

        #region Properties
        public static int GsmReadCycleSeconds { get; set; } = 60;

        #endregion

        #region Events
        /// <summary>
        /// Event SMS empfangen
        /// </summary>
        public static event EventHandler<Sms> SmsRecievedEvent;

        /// <summary>
        /// Trigger für das Event SMS empfangen
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsRecievedEvent(Sms e)
        {
            SmsRecievedEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<Sms> SmsSentEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsSentEvent(Sms e)
        {
            SmsSentEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<Sms> SmsStatusreportEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseSmsStatusreportEvent(Sms e)
        {
            SmsStatusreportEvent?.Invoke(null, e);
        }

        /// <summary>
        /// Event SMS erfolgreich (mit Empfangsbestätigung) versendet
        /// </summary>
        public static event EventHandler<GsmEventArgs> GsmSignalQualityEvent;

        /// <summary>
        /// Trigger für das Event SMS erfolgreich versendet
        /// </summary>
        /// <param name="e"></param>
        static void RaiseGsmSignalQualityEvent(GsmEventArgs e)
        {
            GsmSignalQualityEvent?.Invoke(null, e);
        }
        #endregion

        #region Constructor
        public static void Connect()
        {
            Gsm_Basics.GsmEvent += ParseGsmRecEvent;
            Gsm_Basics.GsmConnected += SetupGsm;
            Gsm_Basics.Connect();
        }
        #endregion

        private static void SetupGsm(object sender, GsmEventArgs e)
        {
            //Nur beim Verbinden ausführen
            if (e.Type != GsmEventArgs.Telegram.GsmConnection || !e.Message.ToLower().Contains("verbunden")) return;

            System.Threading.Thread.Sleep(2000); //Angstpause

            //Test, ob Modem antwortet
            Gsm_Basics.AddAtCommand("AT");

            Gsm_Basics.AddAtCommand("AT^SSET=1");

            //Set  AT+CMEE =2 to enable extended error text. 
            Gsm_Basics.AddAtCommand("AT+CMEE=2");

            //Erzwinge, dass bei Fehlerhaftem SMS-Senden "+CMS ERROR: <err>" ausgegeben wird statt "OK"
            Gsm_Basics.AddAtCommand("AT^SM20=0,0");

            //Benachrichtigung bei SIM-Karten erkannt oder Sim-Slot offen/zu
            Gsm_Basics.AddAtCommand("AT^SCKS=1");
            Gsm_Basics.AddAtCommand("AT^SCKS?");

            //EIgene Telefonnumer-Nummer der SIM-Karte auslesen 
            Gsm_Basics.AddAtCommand("AT+CNUM");

            //SIM-Karte im Mobilfunknetz registriert?
            Gsm_Basics.AddAtCommand("AT+CREG=1");

            //Name Mobilfunknetz?
            Gsm_Basics.AddAtCommand("AT+COPS?");

            //Signalqualität
            Gsm_Basics.AddAtCommand("AT+CSQ");

            //TEST Zeit zwischen setzen und Abfragen notwendig?
            Gsm_Basics.AddAtCommand("AT+CREG?");

            //Textmode
            Gsm_Basics.AddAtCommand("AT+CMGF=1");

            //SendATCommand("AT+CPMS=\"SM\""); //ME, SM, MT
            //SendATCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");
            Gsm_Basics.AddAtCommand("AT+CPMS=\"MT\",\"MT\",\"MT\"");

            //Sendeempfangsbestätigungen abonieren
            //Quelle: https://www.codeproject.com/questions/271002/delivery-reports-in-at-commands
            //Quelle: https://www.smssolutions.net/tutorials/gsm/sendsmsat/
            //AT+CSMP=<fo> [,  <vp> / <scts> [,  <pid> [,  <dcs> ]]]
            // <fo> First Octet:
            // <vp> Validity-Period: 0 - 143 (vp+1 x 5min), 144 - 167 (12 Hours + ((VP-143) x 30 minutes)), [...]
            Gsm_Basics.AddAtCommand("AT+CSMP=49,1,0,0");

            Gsm_Basics.AddAtCommand("AT+CNMI=2,1,2,2,1");
            //möglich AT+CNMI=2,1,2,2,1


            //Rufumleitung BAUSTELLE
            Gsm_Basics.AddAtCommand("ATD>**61*+" + Properties.Settings.Default.RelayIncomingCallsTo + "**30#");

            ReadGsmMemory();

            //Startet Timer zum wiederholten Abrufen von Nachrichten
            SetCyclicTimer();

            Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "GSM-Setup wird ausgeführt.");
        }

        /// <summary>
        /// Waretet eine Zeit und stößt dann das Lesen des GSM-Speichers an
        /// </summary>
        internal static void SetCyclicTimer()
        {
            Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Erste Abfrage: " + DateTime.Now.AddSeconds(GsmReadCycleSeconds));

            System.Timers.Timer aTimer = new System.Timers.Timer(GsmReadCycleSeconds * 1000); //sec
            aTimer.Elapsed += (sender, eventArgs) =>
            {                
                Gsm_Basics.AddAtCommand("AT+CREG?");
                Gsm_Basics.AddAtCommand("AT+CSQ");
                ReadGsmMemory();
                Gsm_Basics.RaiseGsmEvent(GsmEventArgs.Telegram.GsmSystem, "Nächste Abfrage: " + DateTime.Now.AddSeconds(GsmReadCycleSeconds));
            };
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


    }
}
