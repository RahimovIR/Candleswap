using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FSharp.Control;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using WebSocket.Uniswap.Infrastructure;
using System.Threading;

namespace WebSocket.Uniswap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandlesController : ControllerBase
    {
        [HttpGet()]
        public async Task<object> GetHistoricalCandles([FromQuery] string symbol,
            [FromQuery] string interval,
            [FromQuery] long startTime,
            [FromQuery] long endTime,
            [FromQuery] int limit)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return BadRequest("A symbol should be provided");
            }

            if (string.IsNullOrEmpty(interval))
            {
                return BadRequest("An interval should be provided");
            }

            var candles = await global::Program.DB.fetchCandlesTask(symbol, limit);

            return candles;
        }
    }
}
