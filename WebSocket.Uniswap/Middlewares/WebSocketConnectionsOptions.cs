using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocket.Uniswap.Middlewares
{
    internal class WebSocketConnectionsOptions
    {
        public HashSet<string> AllowedOrigins { get; set; }

        public int? SendSegmentSize { get; set; }

        public int ReceivePayloadBufferSize { get; set; }

        public WebSocketConnectionsOptions()
        {
            ReceivePayloadBufferSize = 4 * 1024;
        }
    }
}
