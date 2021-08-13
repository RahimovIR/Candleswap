using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedDuck.Candleswap.Candles;
using RedDuck.Candleswap.Candles.CSharp;

namespace WebSocket.Uniswap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandlesController : ControllerBase
    {
        private readonly ICandleStorageService candleStorage;

        public CandlesController(ICandleStorageService candleStorage)
        {
            this.candleStorage = candleStorage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="periodSeconds"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <example>
        /// curl -X GET "https://localhost:44359/api/Candles?symbol=0xb4e16d0168e52d35cacd2c6185b44281ec28c9dc&periodSeconds=1800&startTime=1625121600&endTime=1625125519&limit=1" -H  "accept: */*"
        /// </example>
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

            var pair = (await candleStorage.FetchPairsAsync()).FirstOrDefault(pair => token0Id == pair.token0Id && 
                                                                                      token1Id == pair.token1Id);
            if (pair == null)
                return BadRequest("There is now such pair");

            /*var startDateTime = startTime == null
                ? DateTimeOffset.MinValue.UtcDateTime
                : DateTimeOffset.FromUnixTimeSeconds(startTime.Value).UtcDateTime;
            var endDateTime = endTime == null
                ? DateTimeOffset.MaxValue.UtcDateTime
                : DateTimeOffset.FromUnixTimeSeconds(endTime.Value).UtcDateTime;*/
            startTime ??= new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() - 60;
            endTime ??= new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            limit ??= 10;

            var candles = await candleStorage.FetchCandlesAsync(pair.id, periodSeconds);

            return candles
                .Where(c => c.datetime >= startTime && c.datetime <= endTime)
                .OrderByDescending(c => c.datetime)
                .Take(limit.Value);
        }
    }
}
