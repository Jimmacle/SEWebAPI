using Newtonsoft.Json;
using Sandbox;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Plugins;

namespace WebAPIPlugin
{
    public sealed class Plugin : IPlugin
    {
        private int rateLimit = 100;
        private bool init = false;

        private APIServer server = new APIServer("http://*:80/");

        public void Init(object obj)
        {
            MySandboxGame.Log.WriteLineAndConsole("SEWA: Initializing SE Web API");
            if (MySandboxGame.ConfigDedicated.Mods.Contains(662477070))
            {
                APIBlockCache.Load();

                //Load API server modules
                server.Handlers.Add(new GridHandler("*/grid"));
                server.Handlers.Add(new BlocksHandler("*/blocks"));
                server.Handlers.Add(new BlockHandler("*/blocks/*"));
                server.Handlers.Add(new GroupsHandler("*/groups"));
                server.Handlers.Add(new InventoryHandler("*/blocks/*/inventory"));

                //TODO: Load extension modules from folder of DLLs

                server.Start();

                MySandboxGame.Log.WriteLineAndConsole($"SEWA: Loaded {server.Handlers.Count} URI handlers");

                MyEntities.OnEntityCreate += MyEntities_OnEntityCreate;
                MyAPIGateway.Multiplayer.RegisterMessageHandler(7331, MessageHandler);
                init = true;
            }
            else
            {
                MySandboxGame.Log.WriteLineAndConsole("SEWA: Mod 662477070 is not installed, SEWA won't function without it!");
            }
        }

        private void MessageHandler(byte[] message)
        {
            long entityId = BitConverter.ToInt64(message, 0);
            var textPanel = MyEntities.GetEntityById(entityId) as IMyTextPanel;

            APIBlockCache.keyDict.Remove(textPanel.GetPrivateText());
            APIBlockCache.Add(textPanel);
        }

        private void MyEntities_OnEntityCreate(MyEntity entity)
        {
            if (entity is MyCubeBlock)
            {
                if ((entity as IMyCubeBlock).BlockDefinition.SubtypeName == "WebAPI")
                {
                    MySandboxGame.Log.WriteLineAndConsole("SEWA: Registering new API block");
                    APIBlockCache.Add(entity as IMyTextPanel);
                }
            }
        }

        public void Update()
        {
            if (!init) return;

            server.ProcessQueue(rateLimit);
        }

        public void Dispose()
        {
            APIBlockCache.Save();
        }
    }
}
