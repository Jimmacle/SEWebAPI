using Newtonsoft.Json;
using Sandbox.Game.Entities.Cube;
using System.Linq;

namespace WebAPIPlugin
{
    public struct WebTerminalBlock
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("id")]
        public long Id;

        public WebTerminalBlock(MyTerminalBlock block)
        {
            this.Name = block.CustomName.ToString();
            this.Type = block.GetType().ToString().Split('.').LastOrDefault();
            this.Id = block.EntityId;
        }
    }
}
