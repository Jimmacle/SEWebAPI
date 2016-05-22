using Newtonsoft.Json;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System.Collections.Generic;
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

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties;

        public WebTerminalBlock(IMyTerminalBlock block, bool includeProperties = true)
        {
            this.Name = block.CustomName.ToString();
            this.Type = block.GetType().ToString().Split('.').LastOrDefault();
            this.Id = block.EntityId;

            if (includeProperties)
            {
                List<ITerminalProperty> props = new List<ITerminalProperty>();
                block.GetProperties(props);

                this.Properties = new Dictionary<string, string>(props.Count);

                foreach (var prop in props)
                {
                    switch (prop.TypeName)
                    {
                        case "Boolean":
                            this.Properties.Add(prop.Id, prop.AsBool().GetValue(block).ToString());
                            break;
                        case "Single":
                            this.Properties.Add(prop.Id, prop.AsFloat().GetValue(block).ToString());
                            break;
                        case "Color":
                            this.Properties.Add(prop.Id, prop.AsColor().GetValue(block).ToString());
                            break;
                    }
                }
            }
            else
            {
                this.Properties = null;
            }
        }
    }
}
