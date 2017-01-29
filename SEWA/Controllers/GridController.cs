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
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace SEWA.Controllers
{
    public class GridController : Controller
    {
        /// <inheritdoc />
        public override async Task GetAsync()
        {
            long id = 0;
            //if (!long.TryParse(request.PathSegments.Last(), out id))
            if (!ValueBag.TryGet("id", out id))
            {
                await Request.RespondAsync("Invalid grid ID", HttpStatusCode.BadRequest);
                return;
            }

            Grid grid = null;
            await Torch.InvokeAsync(() =>
            {
                MyAPIGateway.Entities.TryGetEntityById(id, out IMyEntity entity);
                if (entity is IMyCubeGrid cubeGrid)
                    grid = new Grid(cubeGrid);
            });

            if (grid != null)
            {
                await Request.RespondAsync(grid, HttpStatusCode.OK);
                return;
            }

            await Request.RespondAsync("Grid not found", HttpStatusCode.NotFound);
        }
    }
}
