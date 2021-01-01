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


        //  public static string MasterPassword { get; set; } = "1234";
        #endregion

    }
}
