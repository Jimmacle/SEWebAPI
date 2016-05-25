using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using Sandbox.Definitions;
using VRage.Game.Entity;
using VRage.Game.ModAPI.Ingame;

namespace WebAPIPlugin
{
    public class InventoryHandler : APIHandler
    {
        public InventoryHandler(string path) : base(path) { }

        public override void Get(HttpListenerContext context)
        {
            var uri = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/');
            var apiBlock = APIBlockCache.Get(uri.FirstOrDefault());

            if (apiBlock == null)
            {
                return;
            }

            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((VRage.Game.ModAPI.IMyCubeGrid)apiBlock.CubeGrid);

            var blocks = apiBlock.CubeGrid.GetTerminalBlocks();

            long id = 0;

            if (long.TryParse(uri.ElementAtOrDefault(2), out id))
            {
                var block = blocks.First(b => b.EntityId == id);
                
                if (block != null && block.GetInventoryCount() > 0)
                {
                    var items = new WebInventoryItemCollection();
                    items.Data = new List<WebInventoryItem>();
                    for (var i = 0; i < block.GetInventoryCount(); i++)
                    {
                        var inventory = block.GetInventory(i);
                        foreach (var item in inventory.GetItems())
                        {
                            items.Data.Add(new WebInventoryItem(MyDefinitionManager.Static.GetPhysicalItemDefinition(item.Content).DisplayNameText, (float)item.Amount));
                        }
                    }

                    context.Respond(items, 200);
                }
            }
        }

        public override void Put(HttpListenerContext context)
        {
            context.Respond("501 Not Implemented", 501);
        }
    }
}
