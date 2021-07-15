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
using RedDuck.Candleswap.Candles;

namespace WebSocket.Uniswap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandlesController : ControllerBase
    {
        //curl -X GET "https://localhost:44359/api/Candles?symbol=0xb4e16d0168e52d35cacd2c6185b44281ec28c9dc&periodSeconds=1800&startTime=1625121600&endTime=1625125519&limit=1" -H  "accept: */*"
        [HttpGet()]
        public async Task<object> GetHistoricalCandles([FromQuery] string token0Id,
            [FromQuery] string token1Id,
            [FromQuery] int periodSeconds,
            [FromQuery] long? startTime,
            [FromQuery] long? endTime,
            [FromQuery] int? limit)
        {
            if (string.IsNullOrEmpty(token0Id) || string.IsNullOrEmpty(token1Id))
            {
                return BadRequest("Two tokens should be provided");
            }

            if (periodSeconds == default)
            {
                return BadRequest("An interval should be provided");
            }

            var startDateTime = startTime == null
                ? DateTimeOffset.MinValue.UtcDateTime
                : DateTimeOffset.FromUnixTimeSeconds(startTime.Value).UtcDateTime;
            var endDateTime = endTime == null
                ? DateTimeOffset.MaxValue.UtcDateTime
                : DateTimeOffset.FromUnixTimeSeconds(endTime.Value).UtcDateTime;
            limit ??= 10;

            var candles = await DB.fetchCandlesTask(token0Id, token1Id, periodSeconds);

            return candles.Where(c => c.datetime >= startDateTime
            && c.datetime <= endDateTime)
                .OrderByDescending(c => c.datetime)
                .Take(limit.Value);
        }
    }
}
