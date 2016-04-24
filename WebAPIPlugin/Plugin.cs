using Newtonsoft.Json;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Plugins;

namespace WebAPIPlugin
{
    public sealed class Plugin : IPlugin
    {
        private Thread webThread;
        private ConcurrentQueue<HttpListenerContext> requests = new ConcurrentQueue<HttpListenerContext>();
        private int rateLimit = 1;
        private bool init = false;

        public void Init(object obj)
        {
            MySandboxGame.Log.WriteLineAndConsole("SEWA: Initializing SE Web API");
            if (MySandboxGame.ConfigDedicated.Mods.Contains(662477070))
            {
                APIBlockCache.Load();

                webThread = new Thread(new ThreadStart(WebLoop));
                webThread.Start();

                MyEntities.OnEntityCreate += MyEntities_OnEntityCreate;
                MyAPIGateway.Multiplayer.RegisterMessageHandler(7331, MessageHandler);
                init = true;
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("SEWA: Mod 662477070 is not installed, SEWA won't function without it!");
            }
        }

        private void MessageHandler(byte[] message)
        {
            long entityId = BitConverter.ToInt64(message, 0);
        }

        private void MyEntities_OnEntityCreate(MyEntity entity)
        {
            if (entity is MyCubeBlock)
            {
                if ((entity as IMyCubeBlock).BlockDefinition.SubtypeName == "WebAPI")
                {
                    MySandboxGame.Log.WriteLineAndConsole("SEWA: Registering new API block");
                    APIBlockCache.Add(entity as IMyTextPanel);
                }
            }
        }

        public void Update()
        {
            if (!init) return;

            for (int i = 0; !requests.IsEmpty && i < rateLimit; i++)
            {
                HttpListenerContext context;
                if (requests.TryDequeue(out context))
                {
                    if (context.Request.HttpMethod == "PUT")
                    {
                        WebHandlers.Put(context);
                    }
                    else
                    {
                        context.Respond("400 Bad Request", 400);
                    }
                }
            }
        }

        public void Dispose()
        {
            APIBlockCache.Save();

            if (webThread.IsAlive)
            {
                try
                {
                    webThread.Abort();
                }
                catch (ThreadAbortException) { }
            }
        }

        public void WebLoop()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:80/api/");
            listener.Start();

            while (true)
            {
                var context = listener.GetContext();

                if (context.Request.HttpMethod == "GET")
                {
                    Task.Run(() => WebHandlers.Get(context));
                }
                else
                {
                    requests.Enqueue(context);
                }
            }
        }
    }

    public static class Extensions
    {
        public static void Respond(this HttpListenerContext context, object data, int statusCode)
        {
            try
            {
                var response = context.Response;
                response.StatusCode = statusCode;

                string serialized = JsonConvert.SerializeObject(data, Formatting.Indented);
                byte[] bytes = Encoding.ASCII.GetBytes(serialized);
                response.ContentLength64 = bytes.Length;

                response.OutputStream.Write(bytes, 0, bytes.Length);
                response.OutputStream.Close();
            }
            catch
            {
                MySandboxGame.Log.WriteLineAndConsole("SEWA: Error writing stream");
            }
        }
    }
}
