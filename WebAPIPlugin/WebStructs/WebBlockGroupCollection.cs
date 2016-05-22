using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPIPlugin
{
    public struct WebBlockGroupCollection
    {
        [JsonProperty("data")]
        public List<WebBlockGroup> Data;
    }
}
