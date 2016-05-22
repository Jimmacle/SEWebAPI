using Newtonsoft.Json;

namespace WebAPIPlugin
{
    public struct WebInventoryItem
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("amount")]
        public float Amount;

        [JsonProperty("mass")]
        public float Mass;
    }
}
