using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;

namespace WebAPIPlugin
{
    public class BlockHandler : APIHandler
    {
        public BlockHandler(string path) : base(path) { }

        public override void Get(HttpListenerContext context)
        {
            var uri = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/');
            var apiBlock = APIBlockCache.Get(uri.FirstOrDefault());

            if (apiBlock == null)
            {
                return;
            }

            var gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid((IMyCubeGrid)apiBlock.CubeGrid);

            var blocks = apiBlock.CubeGrid.GetTerminalBlocks();

            long id = 0;

            if (long.TryParse(uri.ElementAtOrDefault(2), out id))
            {
                var block = blocks.First(b => b.EntityId == id);
                if (block != null)
                {
                    context.Respond(new WebTerminalBlock(block as MyTerminalBlock, true), 200);
                }
            }
        }

        public override void Put(HttpListenerContext context)
        {
            context.Respond("501 Not Implemented", 501);
        }
    }
}
