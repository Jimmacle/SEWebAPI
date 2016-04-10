using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using VRage.Plugins;
using System.Windows.Threading;
using System.Threading;
using Sandbox;
using System.Net;
using Sandbox.Game.Entities;
using Sandbox.Engine.Multiplayer;
using Sandbox.ModAPI;
using Sandbox.Game.Entities.Cube;
using VRage.Game.Entity;
using Newtonsoft.Json;
using TermExtensions = Sandbox.ModAPI.Ingame.TerminalBlockExtentions;
using System.Collections.Concurrent;
using System.IO;
using VRage.Game.ModAPI;
using System.Linq.Expressions;

namespace WebAPIPlugin
{
    public class Plugin : IPlugin
    {
        private MySandboxGame gameInstance;
        private Thread webThread;
        private ConcurrentQueue<HttpListenerContext> requests;
        private int rateLimit = 1;

        public void Init(object obj)
        {
            gameInstance = (MySandboxGame)obj;

            APIBlockCache.Load();
            requests = new ConcurrentQueue<HttpListenerContext>();

            webThread = new Thread(new ThreadStart(HTTPLoop));
            webThread.Start();

            MyEntities.OnEntityCreate += MyEntities_OnEntityCreate;
            MyAPIGateway.Multiplayer.RegisterMessageHandler(7331, MessageHandler);
        }

        private void MessageHandler(byte[] message)
        {
            long entityId = BitConverter.ToInt64(message, 0);
        }

        private void MyEntities_OnEntityCreate(MyEntity entity)
        {
            //register api block
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
            HashSet<MyEntity> entities = MyEntities.GetEntities();

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

                        uri.RemoveRange(0, 2);

                        switch (uri.FirstOrDefault())
                        {
                            case "grid":
                                context.Respond(new WebGrid(apiBlock.CubeGrid as MyCubeGrid), 200);
                                break;
                            case "blocks":
                                //Get specific block by id
                                if (request.QueryString.AllKeys.Contains("id"))
                                {
                                    long id = 0;
                                    long.TryParse(request.QueryString["id"], out id);
                                    var block = (apiBlock.CubeGrid as MyCubeGrid).CubeBlocks.First(b => b.FatBlock?.EntityId == id);

                                    if (block != null)
                                    {
                                        context.Respond(new WebTerminalBlock(block.FatBlock as MyTerminalBlock), 200);
                                    }
                                    else
                                    {
                                        context.Respond("Block not found", 404);
                                    }
                                    break;
                                }

                                //Get first block matching name
                                if (request.QueryString.AllKeys.Contains("name"))
                                {
                                    string name = request.QueryString["name"];
                                    var matches = new List<WebTerminalBlock>();
                                    foreach (var block in (apiBlock.CubeGrid as MyCubeGrid).CubeBlocks)
                                    {
                                        var terminalBlock = block?.FatBlock as MyTerminalBlock;
                                        if (terminalBlock != null && terminalBlock.CustomName.ToString() == name)
                                        {
                                            context.Respond(new WebTerminalBlock(terminalBlock), 200);
                                            break;
                                        }
                                    }
                                    context.Respond("Block not found", 404);
                                    break;
                                }
                                //Find all blocks matching search pattern
                                if (request.QueryString.AllKeys.Contains("search"))
                                {
                                    string name = request.QueryString["search"];
                                    var matches = new List<WebTerminalBlock>();
                                    foreach (var block in (apiBlock.CubeGrid as MyCubeGrid).CubeBlocks)
                                    {
                                        var terminalBlock = block?.FatBlock as MyTerminalBlock;
                                        if (terminalBlock != null && terminalBlock.CustomName.ToString().Contains(name, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            matches.Add(new WebTerminalBlock(terminalBlock));
                                        }
                                    }
                                    context.Respond(matches, 200);
                                }
                                break;
                            case "groups":
                                break;
                            default:
                                context.Respond("Resource not found", 404);
                                break;


                        }

                        /*//Begin old/test stuff.
                        Console.WriteLine("Processing request from " + request.RemoteEndPoint.Address.ToString());

                        MyCubeGrid targetGrid = null;
                        if (query.AllKeys.Contains("grid"))
                        {
                            foreach (var entity in entities)
                            {
                                if (entity is MyCubeGrid && entity.DisplayName == query["grid"])
                                {
                                    targetGrid = entity as MyCubeGrid;
                                    break;
                                }
                            }
                        }

                        MyTerminalBlock targetBlock = null;
                        if (targetGrid != null)
                        {
                            if (query.AllKeys.Contains("block"))
                            {
                                foreach (var block in targetGrid.CubeBlocks)
                                {
                                    if (block?.FatBlock is MyTerminalBlock && (block.FatBlock as MyTerminalBlock).CustomName.ToString() == query["block"])
                                    {
                                        targetBlock = block.FatBlock as MyTerminalBlock;
                                        break;
                                    }
                                }
                            }
                        }

                        if (query.AllKeys.Contains("action") && targetBlock != null)
                        {
                            string action = query["action"];
                            if (TermExtensions.HasAction(targetBlock, action))
                            {
                                TermExtensions.ApplyAction(targetBlock, action);
                                context.Respond("OK", 200);
                            }
                            else
                            {
                                context.Respond("Bad Request", 400);
                            }
                        }

                        byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);

                        response.ContentLength64 = responseBytes.Length;

                        response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
                        response.OutputStream.Close();
                        */

                    }

                    i++;
                }
            }
        }

        public void Dispose()
        {
            APIBlockCache.Save();

            try
            {
                webThread.Abort();
            }
            catch (ThreadAbortException) { }
        }

        public void HTTPLoop()
        {
            var listener = new HttpListener();
            listener.Start();
            listener.Prefixes.Add("http://*:80/api/");

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
            byte[] dataBytes = Encoding.ASCII.GetBytes(data);
            var response = context.Response;

            TextWriter tw = new StreamWriter(response.OutputStream);

            response.ContentLength64 = dataBytes.Length;
            response.ContentEncoding = Encoding.ASCII;
            response.StatusCode = statusCode;

            response.OutputStream.Write(dataBytes, 0, dataBytes.Length);
            response.OutputStream.Close();
        }

        public static void Respond(this HttpListenerContext context, object data, int statusCode)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            StreamWriter sw = new StreamWriter(response.OutputStream);
            JsonSerializer s = JsonSerializer.Create();
            s.Serialize(sw, data);
            sw.Close();
        }
    }
}
