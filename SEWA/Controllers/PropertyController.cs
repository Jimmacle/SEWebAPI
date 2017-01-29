using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sandbox.ModAPI;
using SEWA.ViewModels;
using Torch.API;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace SEWA.Controllers
{
    public class PropertyController : Controller
    {
        /// <inheritdoc />
        public override async Task GetAsync()
        {
            var values = GetValues();

            BlockProperty property = null;
            await Torch.InvokeAsync(() =>
            {
                MyAPIGateway.Entities.TryGetEntityById(values.Id, out IMyEntity entity);
                if (entity is IMyTerminalBlock block)
                {
                    property = new BlockProperty(block.GetProperty(values.Property), block);
                }
            });

            if (property != null)
            {
                await Request.RespondAsync(property, HttpStatusCode.OK);
                return;
            }

            await Request.RespondAsync("Block or property not found.", HttpStatusCode.NotFound);
        }

        /// <inheritdoc />
        public override async Task PutAsync()
        {
            if (!Request.QueryString.AllKeys.Contains("value"))
            {
                await Request.RespondAsync("No value supplied.", HttpStatusCode.BadRequest);
            }

            var input = Request.QueryString["value"];
            Log.Debug($"Put input: {input}");
            var values = GetValues();

            bool success = false;
            await Torch.InvokeAsync(() =>
            {
                MyAPIGateway.Entities.TryGetEntityById(values.Id, out IMyEntity entity);
                if (entity is IMyTerminalBlock block)
                {
                    success = block.GetProperty(values.Property).TrySetValue(block, input);
                }
            });

            if (success)
                await Request.RespondAsync("Property set.", HttpStatusCode.OK);
            else
                await Request.RespondAsync("Setting property failed.", HttpStatusCode.InternalServerError);
        }

        public (long Id, string Property) GetValues()
        {
            ValueBag.TryGet("property", out string prop);
            ValueBag.TryGet("id", out long id);

            return (id, prop);
        }
    }
}
