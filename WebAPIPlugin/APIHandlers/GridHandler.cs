using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIPlugin
{
    public class GridHandler : APIHandler
    {
        public GridHandler(string path = "") : base(path) { }

        public override void Get(HttpListenerContext context)
        {
            var uri = context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped).Split('/');

            var block = APIBlockCache.Get(uri.FirstOrDefault());

            if (block != null)
            {
                context.Respond(new WebGrid((MyCubeGrid)block.CubeGrid), 200);
            }
            else
            {
                context.Respond("404 Not Found", 404);
            }
        }

        public override void Put(HttpListenerContext context)
        {
            context.Respond("501 Not Implemented", 501);
        }
    }
}
