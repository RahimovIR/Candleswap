using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using RedDuck.Candleswap.Candles.CSharp;

namespace WebSocket.Uniswap.Infrastructure
{
    public static class CandleEvent
    {
        private static readonly SortedDictionary<(string, int), CancellationTokenSource> EventsInvoked = new();

        public static void SubscribeCandles(
            ILogicService logic,
            string token0Id,
            string token1Id,
            Action<string> onCandle, 
            int resolutionSeconds)
        {
            var uniswapId = string.Join(",", token0Id, token1Id);
            if (EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                return;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds), new CancellationTokenSource());
            }
            EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out var cancelToken);
            
            Task.Run(() =>
                logic.GetCandle(token0Id, token1Id, onCandle, TimeSpan.FromSeconds(resolutionSeconds)));
        }

        public static void SubscribeHistoricalCandles(
            ILogicService logic,
            string token0Id, 
            string token1Id,
            Action<string> onCandle, 
            int resolutionSeconds)
        {
            var uniswapId = string.Join(",", token0Id, token1Id);
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

            Task.Run(() =>
                logic.GetCandle(token0Id, token1Id, onCandle, TimeSpan.FromSeconds(resolutionSeconds)), 
                cancelToken.Token);
        }

        public static void UnsubscribeCandles(string token0Id, string token1Id, int resolutionSeconds)
        {
            if (EventsInvoked.TryGetValue((string.Join(",", token0Id, token1Id), resolutionSeconds), out var cancelToken))
            {
                cancelToken.Cancel();
            }
        }
    }
}