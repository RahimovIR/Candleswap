using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RedDuck.Candleswap.Candles.CSharp;
using WebSocket.Uniswap.Helpers;
using static RedDuck.Candleswap.Candles.Types;

namespace WebSocket.Uniswap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricalCandlesController : ControllerBase
    {
        private readonly ICandleStorageService _candleStorageService;
        private readonly ILogicService _logicService;
        private readonly IDictionary<(Pair, int), CancellationTokenSource> _processingHistoricalCandles;

        public HistoricalCandlesController(
            ICandleStorageService candleStorageService,
            ILogicService logicService,
            IDictionary<(Pair, int), CancellationTokenSource> processingHistoricalCandles)
        {
            _candleStorageService = candleStorageService;
            _logicService = logicService;
            _processingHistoricalCandles = processingHistoricalCandles;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start(
            [FromQuery] string token0Id,
            [FromQuery] string token1Id,
            [FromQuery] int periodSeconds)
        {
            if (string.IsNullOrEmpty(token0Id) || string.IsNullOrEmpty(token1Id))
                return BadRequest("Two tokens should be provided");
            if (periodSeconds == default)
                return BadRequest("An period should be provided");

            var pair = await CandleStorageHelper.GetPairOrCreateNewIfNotExists(_candleStorageService, token0Id, token1Id);

            if (_processingHistoricalCandles.TryGetValue((pair, periodSeconds), out _))
                return Ok(new { message = "Indexing for this pair and period has already been started" });
            else
            {
                var cancellationTokenSource = new CancellationTokenSource();
                _processingHistoricalCandles.Add((pair, periodSeconds), cancellationTokenSource);
                _logicService.GetCandle(pair, _ => { } , TimeSpan.FromSeconds(periodSeconds), cancellationTokenSource.Token);
                return Ok(new { message = "Indexing started successfully" });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel(
            [FromQuery] string token0Id,
            [FromQuery] string token1Id,
            [FromQuery] int periodSeconds)
        {
            if (string.IsNullOrEmpty(token0Id) || string.IsNullOrEmpty(token1Id))
                return BadRequest("Two tokens should be provided");
            if (periodSeconds == default)
                return BadRequest("An period should be provided");

            var pair = await CandleStorageHelper.GetPairOrCreateNewIfNotExists(_candleStorageService, token0Id, token1Id);
            if (_processingHistoricalCandles.TryGetValue((pair, periodSeconds), out var cancelToken))
            {
                cancelToken.Cancel();
                _processingHistoricalCandles.Remove((pair, periodSeconds));
                return Ok(new { message = "Indexing canceled successfully" });
            }
            else
            {
                return BadRequest(new { message = "Indexing for this pair and period has not already been started" });
            }
        }
    }
}
