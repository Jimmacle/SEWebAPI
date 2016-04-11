using Newtonsoft.Json;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Plugins;
using Ingame = Sandbox.ModAPI.Ingame;

namespace WebAPIPlugin
{
    public class Plugin : IPlugin
    {
        private Thread webThread;
        private ConcurrentQueue<HttpListenerContext> requests = new ConcurrentQueue<HttpListenerContext>();
        private int rateLimit = 1;
        private bool init = false;

        public void Init(object obj)
        {
            MySandboxGame.Log.WriteLineAndConsole("Initializing SE Web API (SEWA)");
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
                MySandboxGame.Log.WriteLineAndConsole("Mod 662477070 is not installed, SEWA won't function without it!");
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
                    MySandboxGame.Log.WriteLineAndConsole("Registering new SEWA block");
                    APIBlockCache.Add(entity as IMyTextPanel);
                }
            }
        }

        public void Update()
        {
            if (!init) return;

            int i = 0;
            while (!requests.IsEmpty && i < rateLimit)
            {
                HttpListenerContext context;
                if (requests.TryDequeue(out context))
                {
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    NameValueCollection query = request.QueryString;
                    JsonSerializer serializer = JsonSerializer.Create();

                    var uri = request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/').ToList();

                    if (uri.ElementAtOrDefault(0) == "api")
                    {
                        string key = uri.ElementAtOrDefault(1);

                        IMyTextPanel apiBlock = APIBlockCache.Get(key);

                        if (apiBlock == null)
                        {
                            context.Respond("API key not found", 404);
                            continue;
                        }

                        MyCubeGrid grid = (MyCubeGrid)apiBlock.CubeGrid;
                        var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);

                        uri.RemoveRange(0, 2);

                        switch (uri.FirstOrDefault())
                        {
                            case "grid":
                                context.Respond(new WebGrid(apiBlock.CubeGrid as MyCubeGrid), 200);
                                break;
                            case "blocks":
                                List<Ingame.IMyTerminalBlock> blocks = new List<Ingame.IMyTerminalBlock>();
                                gts.GetBlocks(blocks);
                                var blocksCopy = blocks.ToList();

                                //Filter out blocks not in group.
                                if (query.AllKeys.Contains("group"))
                                {
                                    var group = gts.GetBlockGroupWithName(query["group"]);
                                    if (group != null)
                                    {
                                        foreach (var block in blocksCopy)
                                        {
                                            if (!group.Blocks.Contains(block))
                                            {
                                                blocks.Remove(block);
                                            }
                                        }
                                    }
                                }

                                //Filter out blocks that aren't the specified type.
                                if (query.AllKeys.Contains("type"))
                                {
                                    string type = query["type"];
                                    foreach (var block in blocksCopy)
                                    {
                                        if (!block.GetType().ToString().Split('.').Last().Contains(type, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            blocks.Remove(block);
                                        }
                                    }
                                }

                                //Filter out blocks not matching search pattern.
                                if (query.AllKeys.Contains("search"))
                                {
                                    string name = query["search"];
                                    foreach (var block in blocksCopy)
                                    {
                                        if (!block.CustomName.ToString().Contains(name, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            blocks.Remove(block);
                                        }
                                    }
                                }

                                //Get first block matching name.
                                if (query.AllKeys.Contains("name"))
                                {
                                    string name = query["name"];
                                    var block = blocks.Find(b => b.CustomName.ToString() == name);
                                    if (block != null)
                                    {
                                        blocks = new List<Ingame.IMyTerminalBlock>() { block };
                                    }
                                }

                                //Get specific block by id.
                                if (query.AllKeys.Contains("id"))
                                {
                                    long id = 0;
                                    long.TryParse(query["id"], out id);

                                    var block = blocks.First(b => b.EntityId == id);

                                    if (block != null)
                                    {
                                        blocks = new List<Ingame.IMyTerminalBlock>() { block };
                                    }
                                }

                                List<WebTerminalBlock> responseBlocks = new List<WebTerminalBlock>();
                                blocks.ForEach(x => responseBlocks.Add(new WebTerminalBlock(x as MyTerminalBlock)));

                                context.Respond(responseBlocks, 200);
                                break;
                            case "groups":
                                if (query.AllKeys.Contains("name"))
                                {
                                    var group = gts.GetBlockGroupWithName(query["name"]);
                                    if (group != null)
                                    {
                                        var resp = new List<WebTerminalBlock>();
                                        foreach (var block in group.Blocks)
                                        {
                                            resp.Add(new WebTerminalBlock(block as MyTerminalBlock));
                                        }
                                        context.Respond(resp, 200);
                                    }
                                }
                                break;
                            default:
                                context.Respond("Resource not found", 404);
                                break;
                        }
                    }

                    i++;
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
                requests.Enqueue(context);
            }
        }
    }

    public static class Extensions
    {
        public static void Respond(this HttpListenerContext context, string data, int statusCode)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentLength64 = data.Length * sizeof(char);

            TextWriter tw = new StreamWriter(response.OutputStream);
            tw.Write(data);
            tw.Close();
        }

        public static void Respond(this HttpListenerContext context, object data, int statusCode)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            StreamWriter sw = new StreamWriter(response.OutputStream);

            var settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.StringEscapeHandling = StringEscapeHandling.Default;

            JsonSerializer s = JsonSerializer.Create(settings);
            s.Serialize(sw, data);
            sw.Close();
        }
    }
}
