using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Sandbox.ModAPI;

namespace WebAPIPlugin
{
    public struct WebBlockGroup
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("blocks")]
        public List<WebTerminalBlock> Blocks;

        public WebBlockGroup(IMyBlockGroup group)
        {
            this.Name = group.Name;

            this.Blocks = new List<WebTerminalBlock>();
            var groupBlocks = new List<IMyTerminalBlock>();
            group.GetBlocks(groupBlocks);

            foreach (var block in groupBlocks)
            {
                Blocks.Add(new WebTerminalBlock(block));
            }
        }
    }
}
