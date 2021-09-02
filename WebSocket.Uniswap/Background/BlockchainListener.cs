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

        private readonly int[] _defaultPeriods = { 10, 15, 60, 240, 480, 960 };

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
            DoWork(cancellationToken);
        }

        private async Task DoWork(CancellationToken cancellationToken)
        {
            //var lastBlockInBlockchain = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            
            var startFrom = DateTime.UtcNow;

            var lastBlockNumberInBlockchain = await _logicService.GetBlockNumberByDateTimeAsync(false, startFrom);

            var pancakeLauchDateTimestamp = new DateTime(2020, 9, 20, 0, 0, 0);
            var tmp = new DateTime(2021, 9, 2, 10, 39, 0);

            /*_indexerService.IndexInRangeParallel(lastBlockNumberInBlockchain.Value,
                                                 0,
                                                 FSharpOption<BigInteger>.None);*/

            await Task.Delay(TimeSpan.FromSeconds(5));

            //_indexerService.IndexNewBlockAsync(3);


            foreach(var period in _defaultPeriods)
            {
                var periods = _logicService.GetTimeSamples((startFrom, tmp), TimeSpan.FromSeconds(period));

                var blockPeriods = new List<(HexBigInteger, HexBigInteger)>();
                foreach(var (start, end) in periods)
                {
                    var startBlockNumber = await _logicService.GetBlockNumberByDateTimeAsync(false, start);
                    var endBlockNumber = await _logicService.GetBlockNumberByDateTimeAsync(false, end);
                    blockPeriods.Add((startBlockNumber, endBlockNumber));
                }
                var t = 0;
                await _logicService.GetCandles(_ => { }, cancellationToken, blockPeriods);
                /*await Task.Run(() => {
                    _logicService.GetCandles(_ => { }, cancellationToken, blockPeriods);
                    _logicService.GetCandle(WebSocketConnection.OnCandleUpdateReceived, TimeSpan.FromSeconds(period),
                                        cancellationToken);
                });*/
            }
        }

    }
}
