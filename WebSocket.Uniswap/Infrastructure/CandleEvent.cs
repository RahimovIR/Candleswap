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
            var pair = await GetPairOrCreateNewIfNotExists(candleStorageService, token0Id, token1Id);

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
            var pair = await GetPairAsync(candleStorage, token0Id, token1Id);

            if (EventsInvoked.TryGetValue((channel, pair.id, resolutionSeconds), out var cancelToken))
                cancelToken.Cancel();
        }

        public static async Task<Pair> GetPairAsync(ICandleStorageService candleStorage, string token0Id, string token1Id)
        {
            var pairs = await candleStorage.FetchPairsAsync();
            return pairs.FirstOrDefault(pair => token0Id == pair.token0Id && token1Id == pair.token1Id);
        }

        public static async Task<Pair> GetPairOrCreateNewIfNotExists(ICandleStorageService candleStorage, string token0Id, string token1Id)
        {
            var pair = await GetPairAsync(candleStorage, token0Id, token1Id);
            
            if(pair == null)
            {
                await candleStorage.AddPairAsync(new Pair(0, token0Id, token1Id));
                var newPair = (await candleStorage.FetchPairsAsync()).FirstOrDefault();
                return newPair;
            }
            else
                return pair;
        }
    }
}