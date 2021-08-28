using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedDuck.Candleswap.Candles.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Uniswap.Infrastructure;

namespace WebSocket.Uniswap.Background
{
    public class Indexer: BackgroundService
    {
        private readonly ILogger<Indexer> _logger;
        private readonly ILogicService _logicService;
        
        public Indexer(ILogger<Indexer> logger, ILogicService logicService)
        {
            _logger = logger;
            _logicService = logicService;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Indexer running.");
            await DoWork(cancellationToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            var startFrom = DateTimeOffset.UtcNow.DateTime;
            var period = 15;
            /*_logicService.GetCandles(_ => { }, TimeSpan.FromSeconds(period), cancellationToken,
                                         startFrom);*/
            _logicService.GetCandle(WebSocketConnection.OnCandleUpdateReceived, TimeSpan.FromSeconds(period), 
                                    cancellationToken);
        }

    }
}
