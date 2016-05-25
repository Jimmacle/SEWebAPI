using Newtonsoft.Json;

namespace WebAPIPlugin
{
    public struct WebInventoryItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("amount")]
        public float Amount;

        //[JsonProperty("mass")]
        //public float Mass;

        public WebInventoryItem(string name, float amount)
        {
            Name = name;
            Amount = amount;
            //Mass = mass;
        }
    }
}
