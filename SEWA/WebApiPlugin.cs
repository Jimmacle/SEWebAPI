using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Torch;
using Torch.API;

namespace SEWA
{
    [Plugin("SE Web API", "0.0.0.1", "c3cf8255-18dd-455a-8565-6fad78837acb")]
    public class WebApiPlugin : TorchPluginBase
    {
        private SewaServer _server;
        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            LogManager.GetLogger("SEWA").Info("Init");
            _server = new SewaServer(torch);
            _server.Run();
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            _server.Stop();
        }
    }
}
