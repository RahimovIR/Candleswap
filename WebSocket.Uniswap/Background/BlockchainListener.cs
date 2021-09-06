using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FSharp.Core;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using RedDuck.Candleswap.Candles.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Uniswap.Infrastructure;  

namespace WebSocket.Uniswap.Background
{
    public class BlockchainListener: BackgroundService
    {
        private readonly ILogger<BlockchainListener> _logger;
        private readonly ILogicService _logicService;
        private readonly IIndexerService _indexerService;
        private readonly IWeb3 _web3;

        private readonly int[] _defaultPeriods = { 30, 60 };

        public BlockchainListener(ILogger<BlockchainListener> logger, ILogicService logicService,
                                  IIndexerService indexerService, IWeb3 web3)
        {
            _logger = logger;
            _logicService = logicService;
            _indexerService = indexerService;
            _web3 = web3;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Indexer running.");
            await DoWork(cancellationToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            //var lastBlockInBlockchain = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            
            var startFrom = DateTime.UtcNow;

            var lastBlockNumberInBlockchain = await _logicService.GetBlockNumberByDateTimeAsync(false, startFrom);

            var pancakeLauchDateTimestamp = new DateTime(2020, 9, 20, 0, 0, 0);

            var events = new Dictionary<(DateTime, DateTime), AutoResetEvent>();

             _indexerService.IndexInRangeParallel(lastBlockNumberInBlockchain.Value,
                                                 0,
                                                 FSharpOption<BigInteger>.None);

             _indexerService.IndexNewBlockAsync(5);

            foreach(var period in _defaultPeriods)
            {
                var timeSamples =  _logicService.GetTimeSamples((startFrom, pancakeLauchDateTimestamp), TimeSpan.FromSeconds(period));
                foreach (var timeSample in timeSamples)
                    events.Add(timeSample, new AutoResetEvent(false));
                _logicService.GetCandles(_ => { }, cancellationToken, 
                                         (startFrom, pancakeLauchDateTimestamp), TimeSpan.FromSeconds(period));
                _logicService.GetCandle(WebSocketConnection.OnCandleUpdateReceived, TimeSpan.FromSeconds(period),
                    cancellationToken);
            }


        }

    }
}
