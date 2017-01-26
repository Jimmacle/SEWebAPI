using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SEWA.Controllers;
using Torch.API;

namespace SEWA
{
    public class SewaServer
    {
        private ITorchBase _torch;
        private HttpListener _listener;
        private CancellationToken _token;
        private CancellationTokenSource _tokenSource;
        private Router _router;
        private static Logger _log = LogManager.GetLogger("SEWA");

        public SewaServer(ITorchBase torch)
        {
            _torch = torch;
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:8080/");
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            _router = new Router(torch);

            _router.AddRoute("block/{id}", typeof(BlockController));
            _router.AddRoute("grid/{id}", typeof(GridController));
        }

        public async Task Run()
        {
            await Task.Run(() =>
            {
                _log.Info("Starting HTTP listener");
                try
                {
                    _listener.Start();
                    while (!_token.IsCancellationRequested)
                    {
                        var context = _listener.GetContext();
                        _log.Info($"Handling request: {context.Request.RawUrl}");
                        HandleRequestAsync(context);
                    }
                }
                catch (HttpListenerException e)
                {
                    _log.Error("Error in HTTP listener.");
                    _log.Error(e);
                }

                _log.Info("Listen loop cancelled.");
            }, _token);
        }

        public void Stop()
        {
            _log.Info("Stopping listener");
            _tokenSource.Cancel();
            _listener.Stop();
        }

        public async Task HandleRequestAsync(HttpListenerContext context)
        {
            _log.Debug("Handling request.");
            var sw = Stopwatch.StartNew();
            await _router.RouteRequest(new Request(context));
            sw.Stop();
            _log.Debug($"Handled request in {sw.Elapsed.TotalMilliseconds}ms");
        }
    }
}
