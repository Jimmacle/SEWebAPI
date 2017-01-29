using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using SEWA.ViewModels;
using Torch.API;
using VRage.ModAPI;

namespace SEWA.Controllers
{
    public class BlockController : Controller
    {
        /// <inheritdoc />
        public override async Task GetAsync()
        {
            long id = 0;
            //if (!long.TryParse(request.PathSegments.Last(), out id))
            if (!ValueBag.TryGet("id", out id))
            {
                await Request.RespondAsync("Invalid block ID", HttpStatusCode.BadRequest);
                return;
            }

            TerminalBlock block = null;
            await Torch.InvokeAsync(() =>
            {
                MyAPIGateway.Entities.TryGetEntityById(id, out IMyEntity entity);
                if (entity is IMyTerminalBlock terminalBlock)
                    block = new TerminalBlock(terminalBlock, false);
            });

            if (block != null)
            {
                await Request.RespondAsync(block, HttpStatusCode.OK);
                return;
            }

            await Request.RespondAsync("Block not found", HttpStatusCode.NotFound);
        }
    }
}
