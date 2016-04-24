using Newtonsoft.Json;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.Gui;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using Ingame = Sandbox.ModAPI.Ingame;

namespace WebAPIPlugin
{
    public static class WebHandlers
    {
        public static void Get(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            NameValueCollection query = request.QueryString;

            var uri = request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/').ToList();

            if (uri.ElementAtOrDefault(0) == "api")
            {
                string key = uri.ElementAtOrDefault(1);

                IMyTextPanel apiBlock = APIBlockCache.Get(key);

                if (apiBlock == null)
                {
                    context.Respond("API key not found", 404);
                    return;
                }

                MyCubeGrid grid = (MyCubeGrid)apiBlock.CubeGrid;
                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                List<Ingame.IMyTerminalBlock> blocks = new List<Ingame.IMyTerminalBlock>();
                gts.GetBlocks(blocks);
                long id = 0;

                uri.RemoveRange(0, 2);

                switch (uri.FirstOrDefault())
                {
                    case "grid":
                        context.Respond(new WebGrid(apiBlock.CubeGrid as MyCubeGrid), 200);
                        break;
                    case "blocks":
                        List<Ingame.IMyTerminalBlock> matches = new List<Ingame.IMyTerminalBlock>(blocks.Count);

                        Ingame.IMyBlockGroup group = null;
                        string type = "";
                        string search = "";
                        string name = "";

                        //Parse all possible query parameters
                        if (query.AllKeys.Contains("group"))
                        {
                            group = gts.GetBlockGroupWithName(query["group"]);
                        }

                        if (query.AllKeys.Contains("type"))
                        {
                            type = query["type"];
                        }

                        if (query.AllKeys.Contains("search"))
                        {
                            search = query["search"];
                        }

                        if (query.AllKeys.Contains("name"))
                        {
                            name = query["name"];
                        }

                        if (query.AllKeys.Contains("id"))
                        {
                            long.TryParse(query["id"], out id);
                        }

                        //Find blocks that match all supplied filters
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            var block = blocks[i];

                            if (id != 0)
                            {
                                if (block.EntityId == id)
                                {
                                    matches.Add(block);
                                    break;
                                }
                                else continue;
                            }

                            if (name != string.Empty)
                            {
                                if (block.CustomName.ToString() == name)
                                {
                                    matches.Add(block);
                                    break;
                                }
                                else continue;
                            }

                            if (group == null || group.Blocks.Contains(block))
                            {
                                if (type == string.Empty || block.GetType().ToString().Split('.').Last().Contains(type, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (search == string.Empty || block.CustomName.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        matches.Add(block);
                                    }
                                }
                            }
                        }

                        List<WebTerminalBlock> responseBlocks = new List<WebTerminalBlock>(matches.Count);
                        matches.ForEach(x => responseBlocks.Add(new WebTerminalBlock(x as MyTerminalBlock)));

                        context.Respond(responseBlocks, 200);
                        break;
                    case "groups":
                        if (query.AllKeys.Contains("name"))
                        {
                            var bGroup = gts.GetBlockGroupWithName(query["name"]);
                            if (bGroup != null)
                            {
                                var resp = new List<WebTerminalBlock>();
                                foreach (var block in bGroup.Blocks)
                                {
                                    resp.Add(new WebTerminalBlock(block as MyTerminalBlock));
                                }
                                context.Respond(resp, 200);
                            }
                        }
                        break;
                    default:
                        if (long.TryParse(uri.FirstOrDefault(), out id))
                        {
                            var block = blocks.First(b => b.EntityId == id);
                            if (block != null)
                            {
                                context.Respond(new WebTerminalBlock(block as MyTerminalBlock, true), 200);
                                break;
                            }
                        }
                        else
                        {
                            var bGroup = gts.GetBlockGroupWithName(uri.FirstOrDefault());
                            if (bGroup != null)
                            {
                                var resp = new List<WebTerminalBlock>();
                                foreach (var block in bGroup.Blocks)
                                {
                                    resp.Add(new WebTerminalBlock(block as MyTerminalBlock));
                                }
                                context.Respond(resp, 200);
                                break;
                            }
                        }
                        context.Respond("404 Not Found", 404);
                        break;
                }
            }
        }

        public static void Put(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            NameValueCollection query = request.QueryString;

            var uri = request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/').ToList();

            if (uri.ElementAtOrDefault(0) == "api")
            {
                string key = uri.ElementAtOrDefault(1);

                IMyTextPanel apiBlock = APIBlockCache.Get(key);

                if (apiBlock == null)
                {
                    context.Respond("API key not found", 404);
                    return;
                }

                WebPutCollection puts = new WebPutCollection();

                try
                {
                    puts = JsonConvert.DeserializeObject<WebPutCollection>(new StreamReader(request.InputStream).ReadToEnd());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    context.Respond("400 Bad Request", 400);
                    return;
                }

                MyCubeGrid grid = (MyCubeGrid)apiBlock.CubeGrid;
                var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
                List<Ingame.IMyTerminalBlock> blocks = new List<Ingame.IMyTerminalBlock>();
                gts.GetBlocks(blocks);

                uri.RemoveRange(0, 2);

                switch (uri.FirstOrDefault())
                {
                    case "grid":

                        break;
                    case "blocks":
                        if (puts.Puts != null)
                        {
                            for (int i = 0; i < puts.Puts.Length; i++)
                            {
                                var put = puts.Puts[i];
                                var block = blocks.First(b => b.EntityId == put.Id);

                                if (!string.IsNullOrEmpty(put.Action))
                                {
                                    if (Ingame.TerminalBlockExtentions.HasAction(block, put.Action))
                                    {
                                        block.ApplyAction(put.Action);
                                    }
                                }

                                if (!string.IsNullOrWhiteSpace(put.Property) && !string.IsNullOrWhiteSpace(put.PropertyValue))
                                {
                                    var property = block.GetProperty(put.Property);

                                    if (property != null)
                                    {
                                        MySandboxGame.Log.WriteLineAndConsole(property.TypeName);
                                        switch (property.TypeName)
                                        {
                                            case "Boolean":
                                                bool b;
                                                if (bool.TryParse(put.PropertyValue, out b))
                                                {
                                                    property.AsBool().SetValue(block, b);
                                                }
                                                break;
                                            case "Single":
                                                float f;
                                                if (float.TryParse(put.PropertyValue, out f))
                                                {
                                                    property.AsFloat().SetValue(block, f);
                                                }
                                                break;
                                            case "Color":
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        context.Respond("200 OK", 200);
                        break;
                    case "groups":

                        break;
                    default:
                        context.Respond("400 Bad Request", 400);
                        break;
                }
            }
        }
    }
}
