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
        private static readonly SortedDictionary<(string, int), CancellationTokenSource> EventsInvoked = new();

        public async static Task SubscribeCandles(
            ILogicService logic,
            ICandleStorageService candleStorageService,
            string token0Id,
            string token1Id,
            Action<string> onCandle, 
            int resolutionSeconds)
        {
            var pair = await GetPairOrCreateNewIfNotExists(candleStorageService, token0Id, token1Id);

            var uniswapId = string.Join(",", pair.token0Id, pair.token1Id);
            if (EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                return;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds), new CancellationTokenSource());
            }
            EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out var cancelToken);
            
            await Task.Run(() =>
                logic.GetCandle(pair, onCandle, TimeSpan.FromSeconds(resolutionSeconds)));
        }

        public async static Task SubscribeHistoricalCandles(
            ILogicService logic,
            ICandleStorageService candleStorageService,
            string token0Id, 
            string token1Id,
            Action<string> onCandle, 
            int resolutionSeconds)
        {
            var pair = await GetPairOrCreateNewIfNotExists(candleStorageService, token0Id, token1Id);
            var uniswapId = string.Join(",", pair.token0Id, pair.token1Id);
            if (EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                return;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds), new CancellationTokenSource());
            }
            EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out var cancelToken);
            var fsharpFunc = FuncConvert.ToFSharpFunc<string>(c =>
            {
                onCandle(c);
            });
            var web3 = new Nethereum.Web3.Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e");

            await Task.Run(() =>
                logic.GetCandles(pair, onCandle, TimeSpan.FromSeconds(resolutionSeconds)), 
                cancelToken.Token);
        }

        public static void UnsubscribeCandles(string token0Id, string token1Id, int resolutionSeconds)
        {
            if (EventsInvoked.TryGetValue((string.Join(",", token0Id, token1Id), resolutionSeconds), out var cancelToken))
            {
                cancelToken.Cancel();
            }
        }

        public static async Task<Pair> GetPairOrCreateNewIfNotExists(ICandleStorageService candleStorage, string token0Id, string token1Id)
        {
            var pairs = await candleStorage.FetchPairsAsync();

            var pair = pairs.FirstOrDefault(pair => token0Id == pair.token0Id && token1Id == pair.token1Id);
            
            if(pair == null)
            {
                await candleStorage.AddPairAsync(new Pair(0, token0Id, token1Id));
                var newPair = (await candleStorage.FetchPairsAsync()).FirstOrDefault();
                return newPair;
            }
            else
            {
                return pair;
            }
        }
    }
}