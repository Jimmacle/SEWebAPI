using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebAPIPlugin
{
    public struct WebPutCollection
    {
        [JsonProperty("requests")]
        public WebPut[] Puts;
    }
}
