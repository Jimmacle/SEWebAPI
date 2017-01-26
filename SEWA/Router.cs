using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SEWA.Controllers;
using Torch.API;

namespace SEWA
{
    public class Router
    {
        private ITorchBase _torch;
        private readonly Dictionary<string[], Type> _routes = new Dictionary<string[], Type>();
        private static Logger _log = LogManager.GetLogger("SEWA");

        public Router(ITorchBase torchBase)
        {
            _torch = torchBase;
        }

        public void AddRoute(string route, Type controllerType)
        {
            var splitRoute = route.Split('/');
            if (_routes.ContainsKey(splitRoute))
                throw new InvalidOperationException("Route is already regsistered.");

            _routes.Add(splitRoute, controllerType);
        }

        public async Task RouteRequest(Request request)
        {
            //hack for testing
            //if (string.IsNullOrEmpty(request.ApiKey))
            //{
            //    request.Respond("No API key", HttpStatusCode.Forbidden);
            //    return;
            //}

            _log.Debug($"Trying to match route '{string.Join("/", request.PathSegments)}'");

            if (_routes.ContainsKey(request.PathSegments))
            {
                var controller = (Controller)Activator.CreateInstance(_routes[request.PathSegments], _torch);
                await controller.HandleRequestAsync(request);
                return;
            }

            var bestRank = 0;
            string[] bestRoute = null;
            Type bestType = null;
            foreach (var route in _routes)
            {
                var rank = RankRoute(route.Key, request.PathSegments);
                _log.Debug($"Route '{string.Join("/", route.Key)}': {rank}");

                if (rank > bestRank)
                {
                    bestRank = rank;
                    bestRoute = route.Key;
                    bestType = route.Value;
                }
            }

            if (bestType == null)
            {
                _log.Debug("No route for request.");
                await request.RespondAsync("No route found.", HttpStatusCode.NotFound);
                return;
            }

            _log.Info($"Using controller {bestType}");
            var bestController = (Controller)Activator.CreateInstance(bestType, _torch);
            bestController.ValueBag = MakeBag(bestRoute, request.PathSegments);
            await bestController.HandleRequestAsync(request);
        }

        /// <summary>
        /// Returns a rank of how suitable the route is for the given path.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="pathSegments"></param>
        /// <returns></returns>
        private int RankRoute(string[] route, string[] pathSegments)
        {
            if (route.Length > pathSegments.Length - 1)
                return -1;

            var rank = 0;
            for (var i = 0; i < route.Length; i++)
            {
                var segment = pathSegments[i + 1].TrimEnd('/');

                if (route[i].StartsWith("{") && route[i].EndsWith("}"))
                    rank++;
                else if (route[i] == segment)
                    rank += 2;
                else
                    return -1;
            }

            return rank;
        }

        private ValueBag MakeBag(string[] route, string[] pathSegments)
        {
            var bag = new ValueBag();

            for (var i = 0; i < route.Length; i++)
            {
                var part = route[i];
                if (part.StartsWith("{") && part.EndsWith("}"))
                {
                    var name = part.Substring(1, part.Length - 2);
                    var segment = pathSegments[i + 1].TrimEnd('/');
                    bag.Add(name, segment);
                }
            }

            return bag;
        }
    }
}
