using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VRage.Plugins;
using System.Windows.Threading;
using System.Threading;
using Sandbox;
using System.Net;

namespace WebAPIPlugin
{
    public class Plugin : IPlugin
    {
        private MySandboxGame gameInstance;

        public void Init(object obj)
        {
            gameInstance = (MySandboxGame)obj;

            Thread netThread = new Thread(new ThreadStart(NetHandler));
            netThread.Start();
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }

        public void NetHandler()
        {
            var handler = new NetHandler();
            
            while(true)
            {
                handler.HandleRequest();
                gameInstance.Invoke(() => Console.WriteLine("Handled request"));
            }
        }
    }
}
