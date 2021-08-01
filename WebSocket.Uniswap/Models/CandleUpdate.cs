using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocket.Uniswap.Models
{
    public class CandleUpdate
    {
        [JsonProperty("event")]
        public string EventType { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("key")]
        public string KeyParam { get; set; }

    }
}
