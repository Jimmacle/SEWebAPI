using Newtonsoft.Json;
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
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using Ingame = Sandbox.ModAPI.Ingame;

namespace WebAPIPlugin
{
    public class BlocksHandler : APIHandler
    {
        public BlocksHandler(string path) : base(path) { }

        public override void Get(HttpListenerContext context)
        {
            var uri = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/');
            var block = APIBlockCache.Get(uri.FirstOrDefault());

            if (block == null)
            {
                return;
            }

            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((IMyCubeGrid)block.CubeGrid);
            var query = context.Request.QueryString;
            var blocks = block.CubeGrid.GetTerminalBlocks();
            var matches = new List<Ingame.IMyTerminalBlock>(blocks.Count);

            Ingame.IMyBlockGroup group = null;
            string type = "";
            string search = "";
            string name = "";
            long id = 0;

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
                var b = blocks[i];

                if (id != 0)
                {
                    if (b.EntityId == id)
                    {
                        matches.Add(b);
                        break;
                    }
                    else continue;
                }

                if (name != string.Empty)
                {
                    if (b.CustomName.ToString() == name)
                    {
                        matches.Add(b);
                        break;
                    }
                    else continue;
                }

                if (group == null || group.Blocks.Contains(b))
                {
                    if (type == string.Empty || b.GetType().ToString().Split('.').Last().Contains(type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (search == string.Empty || b.CustomName.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase))
                        {
                            matches.Add(b);
                        }
                    }
                }
            }

            List<WebTerminalBlock> responseBlocks = new List<WebTerminalBlock>(matches.Count);
            matches.ForEach(x => responseBlocks.Add(new WebTerminalBlock(x as MyTerminalBlock)));

            context.Respond(responseBlocks, 200);
        }

        public override void Put(HttpListenerContext context)
        {
            var uri = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/');
            var block = APIBlockCache.Get(uri.FirstOrDefault());

            if (block == null)
            {
                context.Respond("API key not found", 404);
                return;
            }

            WebPutCollection requests = new WebPutCollection();

            try
            {
                requests = JsonConvert.DeserializeObject<WebPutCollection>(new StreamReader(context.Request.InputStream).ReadToEnd());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                context.Respond("400 Bad Request", 400);
                return;
            }

            var blocks = block.CubeGrid.GetTerminalBlocks();

            if (requests.Data != null)
            {
                for (int i = 0; i < requests.Data.Count; i++)
                {
                    var put = requests.Data[i];
                    var match = blocks.First(b => b.EntityId == put.Id);

                    if (!string.IsNullOrEmpty(put.Action))
                    {
                        if (Ingame.TerminalBlockExtentions.HasAction(match, put.Action))
                        {
                            match.ApplyAction(put.Action);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(put.Property) && !string.IsNullOrWhiteSpace(put.PropertyValue))
                    {
                        var property = match.GetProperty(put.Property);

                        if (property != null)
                        {
                            switch (property.TypeName)
                            {
                                case "Boolean":
                                    bool b;
                                    if (bool.TryParse(put.PropertyValue, out b))
                                    {
                                        property.AsBool().SetValue(match, b);
                                    }
                                    break;
                                case "Single":
                                    float f;
                                    if (float.TryParse(put.PropertyValue, out f))
                                    {
                                        property.AsFloat().SetValue(match, f);
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
        }
    }
}
