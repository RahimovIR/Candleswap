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
        private static readonly SortedDictionary<(string, long, int), CancellationTokenSource> EventsInvoked = new();

        public async static Task SubscribeCandlesAsync(
            ILogicService logic,
            ICandleStorageService candleStorageService,
            string token0Id,
            string token1Id,
            Action<string> onCandle,
            int resolutionSeconds,
            string channel)
        {
            var pair = await CandleStorageHelper.GetPairOrCreateNewIfNotExists(candleStorageService, token0Id, token1Id);

            if (EventsInvoked.TryGetValue((channel, pair.id, resolutionSeconds), out _))
                return;
            else
                EventsInvoked.Add((channel, pair.id, resolutionSeconds), new CancellationTokenSource());
            EventsInvoked.TryGetValue((channel, pair.id, resolutionSeconds), out var cancelToken);

            if(channel == "candles")
                logic.GetCandle(pair, onCandle, TimeSpan.FromSeconds(resolutionSeconds), cancelToken.Token);
            else if(channel == "historicalCandles")
                logic.GetCandles(pair, onCandle, TimeSpan.FromSeconds(resolutionSeconds), cancelToken.Token);
        }

        public async static Task UnsubscribeCandlesAsync(
            ICandleStorageService candleStorage,
            string token0Id, 
            string token1Id, 
            int resolutionSeconds, 
            string channel)
        {
            var pair = await CandleStorageHelper.GetPairAsync(candleStorage, token0Id, token1Id);

            if (EventsInvoked.TryGetValue((channel, pair.id, resolutionSeconds), out var cancelToken))
                cancelToken.Cancel();
        }
    }
}