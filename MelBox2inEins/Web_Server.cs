using Grapevine.Interfaces.Server;
using Grapevine.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
//using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MelBox2
{
    public partial class MelBoxWeb
    {
        #region WebServer Management
        private static bool RunWebServer = true; //Schalter zum Ausschalten des Webservers

        public static void StartWebServer(int port = 48040)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                var _ = new MelBoxWeb(port);                
            }).Start();
        }

        public static void StopWebServer()
        {
            RunWebServer = false;
        }

        public MelBoxWeb(int port)
        {
            RunWebServer = true;

            using (var server = new RestServer())
            {
                server.Port = PortFinder.FindNextLocalOpenPort(port);
                //server.UseHttps = true;
                server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Warn).Start();
                Console.WriteLine("WebHost:\thttp://" + server.Host + ":" + server.Port);

                while (RunWebServer)
                {
                    //nichts unternehmen 
                }
                server.LogToConsole().Stop();
                server.ThreadSafeStop();
            }
        }

        #endregion

        #region Benutzerverwaltung

        public static Dictionary<string, int> LogedInGuids { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Benutzerverifikation aus Aufrufphad
        /// </summary>
        /// <param name="payload">Aufrufpfad  /seite[/contactid]/guid</param>
        /// <returns>Benutzer-Id</returns>
        public static int LogedInAccountId(string payload)
        {
            string[] urlParts = payload.Split('/');
            string guid = string.Empty;// context.Request.RawUrl.Remove(0, 9);
            int requestedUserId = 0;

            if (urlParts.Length < 3)
            {
                //keine Anmeldeinformationen
                return 0;
            }
            if (urlParts.Length == 3)
            {
                //Format /seite/guid
                guid = urlParts[2];
            }
            else if (urlParts.Length == 4)
            {
                //Format /seite/contactid/guid
                guid = urlParts[3];
                int.TryParse(urlParts[2], out requestedUserId);
            }

            //Nicht angemeldet
            if (!LogedInGuids.ContainsKey(guid))
                return 0;

            //Angemeldeter Benutzer
            int logedInUserId = LogedInGuids[guid];

            //Andere UserId angefragt und angemeldeter Benutzer hat Adminrechte
            if (requestedUserId != 0 && MelBoxSql.AdminIds.Contains(logedInUserId))
                return requestedUserId;

            return logedInUserId;
        }

        #endregion

    }
}
