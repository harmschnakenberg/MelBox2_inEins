using System;

namespace MelBox2
{

    /// <summary>
    /// Container für Melde-Ereignisse
    /// </summary>
    public class GsmEventArgs : EventArgs
    {
        public enum Telegram
        {
            GsmError,
            GsmSystem,
           // GsmStatus,
            GsmConnection,
            GsmSignal,
            GsmOwnPhone,
            GsmRec,
            GsmSent,
            SmsRec,
            SmsStatus,
            SmsSent
        }

        public GsmEventArgs()
        {
            //Benötigt Für JSON Deserialisation
        }

        public GsmEventArgs(Telegram type, string message, object payload)
        {
            Type = type;
            Message = message;
            Payload = payload;
        }

        public Telegram Type { get; set; }

        public string Message { get; set; }

        public object Payload  { get; set; }
    }

    /// <summary>
    /// Container für internes Handling von SMS-Nachrichten 
    /// </summary>
    public class Sms : EventArgs
    {
        #region Beispiele Modem-Antworten
        /* Beispiel: empfangene SMS
         * +CMGL: 1,"REC UNREAD","+4916095285304",,"20/11/16,10:03:13+04"
         * Zyklustest01 16.11.2020 10:03 Test
         * 
         * Beispiel: gesendete SMS
         * +CMGL: 2,"STO SENT","+4916095285304",,
         * Dies ist ein Test Saure Gurke
         * 
         * Beispiel empfangener Statusreport
         * +CMGL: 1,"REC READ",6,34,,,"20/11/06,16:08:45+04","20/11/06,16:08:50+04",0
         * 
         * 
         * +CMGL: <index> ,  <stat> ,  <oa> / <da> , [ <alpha> ], [ <scts> ][,  <tooa> / <toda> ,  <length> ]
         * <data>
         * [... ]
         * OK
         */
        #endregion

        #region SMS

        /// <summary>
        /// index: Index der Nachricht im Modem-Speicher
        /// </summary>
        public byte Index { get; set; }

        /// <summary>
        /// stat: Status-Text von GSM-Modem
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// oa / da Telefonnumer, für Sender / Empfänger (aus Kontext) 
        /// Format mit Ländervorwahl z.B. 49151987654321 (max. 19 Stellen)
        /// </summary>
        public ulong Phone { get; set; }

        /// <summary>
        /// alpha: Texteintrag, wenn in Telefonbuch vorhanden 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// scts: Service Centre Time Stamp
        /// </summary>
        public DateTime SmsProviderTimeStamp { get; set; }

        /// <summary>
        /// toda / tooa: Type of Destination Address / Type of Originating Address
        /// GSM 04.11 TP-Destination-Address Type-of-Address octet in integer format
        /// 129 if the value of address does not start with a "+" character. For example, "85291234567".
        /// 145 if the value of address starts with a "+" character. For example, "+85291234567".
        /// </summary>
        public byte Adresstyp { get; set; }

        /// <summary>
        /// length: Anzahl der Zeichen in der Nachricht
        /// It is 160 characters if the 7 bit GSM coding scheme is used, and 140 characters according to the 8 bit GSM coding scheme.
        /// </summary>
        public int MessageLength { get; set; }

        /// <summary>
        /// data: 
        /// Inhalt der Nachricht
        /// </summary>
        public string Message { get; set; }

        #endregion

        #region Status-Meldungen (Empfangsbestätigung)

        /*
         * +CMGL: <index> ,  <stat> ,  <fo> ,  <mr> , [ <ra> ], [ <tora> ],  <scts> ,  <dt> ,  <st>
         * [... ]
         * OK
         * 
         */

        /// <summary>
        /// fo: First Octet
        /// depending on the command or result code: first octet of GSM 03.40 SMS-DELIVER, SMS-SUBMIT(default 17),
        /// SMS-STATUS-REPORT, or SMS-COMMAND(default 2) in integer format
        /// </summary>        
        public byte FirstOctet { get; set; }

        /// <summary>
        /// mr:  Message-Reference in integer format 
        /// Index der Empfangsbestätigung im Modem-Speicher
        /// </summary>
        public byte MessageReference { get; set; } = 0;

        /// <summary>
        /// st: Status von Sendebestätigung: 
        /// 0-31 erfolfreich von Modem
        /// 32-63 versucht weiter zu senden von Modem
        /// 64-127 sendeversuch abgebrochen von Modem
        /// </summary>
        public byte SendStatus { get; set; } = 255;

        #endregion

        #region Methods

        /// <summary>
        /// TP-Discharge-Time in time-string format: "yy/MM/dd,hh:mm:ss+zz", where characters indicate year
        /// (two last digits), month, day, hour, minutes, seconds and time zone.For example, 6th of May 1994, 22:10:00
        /// GMT+2 hours equals "94/05/06,22:10:00+08" 
        /// </summary>
        /// <param name="strDateTime"></param>
        public void SetTimeStamp(string strDateTime)
        {
            SmsProviderTimeStamp = GsmConverter.ReadDateTime(strDateTime);
        }

        public void SetPhone(string strPhone)
        {
            Phone = GsmConverter.StrToPhone(strPhone);
        }

        public void SetIndex(string strIndex)
        {
            if (byte.TryParse(strIndex, out byte index))
                Index = index;
        }
        #endregion
    }

    public static class GsmConverter
    {
        /// <summary>
        /// Kovertiert einen String mit Zahlen und Zeichen in eine Telefonnumer als Zahl mit führender  
        /// Ländervorwahl z.B. +49 (0) 4201 123 456 oder 0421 123 456 wird zu 49421123456 
        /// </summary>
        /// <param name="str_phone">String, der eine Telefonummer enthält.</param>
        /// <returns>Telefonnumer als Zahl mit führender  
        /// Ländervorwahl (keine führende 00). Bei ungültigem str_phone Rückgabewert 0.</returns>
        public static ulong StrToPhone(string str_phone)
        {
            // Entferne (0) aus +49 (0) 421...
            str_phone = str_phone.Replace("(0)", string.Empty);

            // Entferne alles ausser Zahlen
            System.Text.RegularExpressions.Regex regexObj = new System.Text.RegularExpressions.Regex(@"[^\d]");
            str_phone = regexObj.Replace(str_phone, "");

            // Wenn zu wenige Zeichen übrigbleiben gebe 0 zurück.
            if (str_phone.Length < 2) return 0;

            // Wenn am Anfang 0 steht, aber nicht 00 ersetze führende 0 durch 49
            string firstTwoDigits = str_phone.Substring(0, 2);

            if (firstTwoDigits != "00" && firstTwoDigits[0] == '0')
            {
                str_phone = "49" + str_phone.Substring(1, str_phone.Length - 1);
            }

            ulong number = ulong.Parse(str_phone);

            if (number > 0)
            {
                return number;
            }
            else
            {
                return 0;
            }
        }

        public static DateTime ReadDateTime(string strDateTime)
        {
            if (DateTime.TryParseExact(strDateTime, "yy/MM/dd,hh:mm:ss+zz", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
                return dateTime;
            else
                return DateTime.UtcNow;
        }


        #region JSON

        public static string JSONSerialize(MelBox2.Sms sms)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            return js.Serialize(sms);
        }

        public static string JSONSerialize(MelBox2.GsmEventArgs telegram)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            return js.Serialize(telegram);
        }

        public static MelBox2.Sms JSONDeserializeSms(string json)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            return js.Deserialize<MelBox2.Sms>(json);
        }

        public static MelBox2.GsmEventArgs JSONDeserializeTelegram(string json)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();
            return js.Deserialize<MelBox2.GsmEventArgs>(json);
        }
        #endregion
    }
}
