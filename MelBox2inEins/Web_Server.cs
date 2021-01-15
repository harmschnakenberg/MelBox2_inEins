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
        static AutoResetEvent stopWebServer = new AutoResetEvent(false);

        public static void StartWebServer(int port = 48040)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;

                var _ = new MelBoxWeb(port);
            }).Start();
        }

        public static void StopWebServer()
        {
            stopWebServer.Set();
        }

        public MelBoxWeb(int port)
        {
            using (var server = new RestServer())
            {
                server.Port = PortFinder.FindNextLocalOpenPort(port);
                //server.UseHttps = true;
                server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Warn).Start();
                Console.WriteLine("WebHost:\thttp://" + server.Host + ":" + server.Port);
                
                stopWebServer.WaitOne();
                server.LogToConsole().Stop();
                server.ThreadSafeStop();
            }
        }

        #endregion
    }
}
