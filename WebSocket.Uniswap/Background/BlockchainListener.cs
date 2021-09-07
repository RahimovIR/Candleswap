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

        private readonly int[] _defaultPeriods = { 15, 60, 600 };

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

            foreach(var c in  RedDuck.Candleswap.Candles.Logic2.newCandles(_web3, _logger, lastBlockNumberInBlockchain))
            {
                System.Console.WriteLine($"{c.datetime} {c.resolution} {c.pair.token0Id} {c.pair.token1Id} {c.volume}");
            }


            //var pancakeLauchDateTimestamp = new DateTime(2020, 9, 20, 0, 0, 0);

            //_indexerService.IndexInRangeParallel(lastBlockNumberInBlockchain.Value,
            //                                     0,
            //                                     FSharpOption<BigInteger>.None);

            //_indexerService.IndexNewBlockAsync(5);

            //foreach (var period in _defaultPeriods)
            //    _logicService.GetCandle(WebSocketConnection.OnCandleUpdateReceived, TimeSpan.FromSeconds(period),
            //        cancellationToken);

            //foreach(var period in _defaultPeriods)
            //{
            //    _logicService.GetCandles(_ => { }, cancellationToken, 
            //                             (startFrom, pancakeLauchDateTimestamp), TimeSpan.FromSeconds(period));
            //}


        }

    }
}
