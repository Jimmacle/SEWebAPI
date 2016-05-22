using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ingame = Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI;

namespace WebAPIPlugin
{
    public struct WebBlockGroup
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("blocks")]
        public List<WebTerminalBlock> Blocks;

        public WebBlockGroup(Ingame.IMyBlockGroup group)
        {
            this.Name = group.Name;

            this.Blocks = new List<WebTerminalBlock>();
            foreach (var block in group.Blocks)
            {
                Blocks.Add(new WebTerminalBlock(block as IMyTerminalBlock));
            }
        }
    }
}
