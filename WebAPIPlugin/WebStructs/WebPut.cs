using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebAPIPlugin
{
    public struct WebPut
    {
        [JsonProperty("id")]
        public long Id;

        [JsonProperty("action")]
        public string Action;

        [JsonProperty("property")]
        public string Property;

        [JsonProperty("value")]
        public string PropertyValue;
    }
}
