using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;

namespace WebSocket.Uniswap.Infrastructure
{
    public static class CandleEvent
    {
        private static SortedDictionary<(string, int), CancellationTokenSource> EventsInvoked = new();

        public static void SubscribeCandles(string uniswapId, Action<string> onCandle, int resolutionSeconds)
        {
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

            global::Program.Logic.getCandle(uniswapId, fsharpFunc, TimeSpan.FromSeconds(resolutionSeconds),
                                            web3);

        }

        public static void UnsubscribeCandles(string uniswapId, int resolutionSeconds)
        {
            if (EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out var cancelToken))
            {
                cancelToken.Cancel();
            }
        }
    }
}