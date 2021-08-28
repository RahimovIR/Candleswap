using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using RedDuck.Candleswap.Candles.CSharp;
using WebSocket.Uniswap.Helpers;
using static RedDuck.Candleswap.Candles.Types;

namespace WebSocket.Uniswap.Infrastructure
{
    public static class CandleEvent
    {
        private static readonly SortedDictionary<(Pair, int), CancellationTokenSource> _processingCandles = new();
        public static readonly Dictionary<(Pair, int), List<Guid>> _subscriptions = new(); 

        public async static Task SubscribeCandlesAsync(
            ILogicService logic,
            ICandleStorageService candleStorageService,
            string token0Id,
            string token1Id,
            int resolutionSeconds,
            Guid subscriberId)
        {
            var pair = await CandleStorageHelper.GetPairOrCreateNewIfNotExists(candleStorageService, token0Id, token1Id);

            if (_processingCandles.TryGetValue((pair, resolutionSeconds), out _))
                _subscriptions[(pair, resolutionSeconds)].Add(subscriberId);
            else
            {
                _subscriptions.Add((pair, resolutionSeconds), new List<Guid> { subscriberId });

                var cancelToken = new CancellationTokenSource();
                _processingCandles.Add((pair, resolutionSeconds), cancelToken);
            }
        }

        public async static Task UnsubscribeCandlesAsync(
            ICandleStorageService candleStorage,
            string token0Id, 
            string token1Id, 
            int resolutionSeconds,
            Guid subscriberId)
        {
            var pair = await CandleStorageHelper.GetPairAsync(candleStorage, token0Id, token1Id);

            if(_subscriptions.TryGetValue((pair, resolutionSeconds), out var subscribers))
            {
                subscribers.Remove(subscriberId);
                if(subscribers.Count == 0 && _processingCandles.TryGetValue((pair, resolutionSeconds), out var cancelToken))
                {
                    cancelToken.Cancel();
                    _subscriptions.Remove((pair, resolutionSeconds));
                    _processingCandles.Remove((pair, resolutionSeconds));
                }
            }
        }
    }
}