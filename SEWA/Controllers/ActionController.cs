using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using SEWA.ViewModels;
using Torch.API;
using VRage.ModAPI;
using IMyTerminalBlock = Sandbox.ModAPI.IMyTerminalBlock;

namespace SEWA.Controllers
{
    public class ActionController : Controller
    {
        /// <inheritdoc />
        public override async Task PostAsync()
        {
            long id;
            string actionName;
            if (!(ValueBag.TryGet("id", out id) && ValueBag.TryGet("action", out actionName)))
            {
                await Request.RespondAsync("Invalid parameters.", HttpStatusCode.BadRequest);
                return;
            }

            await Torch.InvokeAsync(() =>
            {
                MyAPIGateway.Entities.TryGetEntityById(id, out IMyEntity entity);
                if (entity is IMyTerminalBlock terminalBlock && terminalBlock.HasAction(actionName))
                    terminalBlock.ApplyAction(actionName);
            });

            await Request.RespondAsync(null, HttpStatusCode.OK);
        }
    }
}
