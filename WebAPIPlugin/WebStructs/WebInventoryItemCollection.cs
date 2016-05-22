using Newtonsoft.Json;
using System.Collections.Generic;

namespace WebAPIPlugin
{
    public struct WebInventoryItemCollection
    {
        [JsonProperty("data")]
        public List<WebInventoryItem> Data;
    }
}
