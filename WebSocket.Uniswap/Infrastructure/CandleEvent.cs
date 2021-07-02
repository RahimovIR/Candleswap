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

        public static SortedSet<(string, int)> EventsInvoked = new SortedSet<(string, int)>();

        public static Task GetCandles(string uniswapId, Action<string> onCandle, int resolutionSeconds)
        {
            if (EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                //return Task.CompletedTask;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds));
            }
            uniswapId = "0xb4e16d0168e52d35cacd2c6185b44281ec28c9dc";
            var fsharpFunc = FuncConvert.ToFSharpFunc<string>(t => onCandle(t));
            var web3 = new Nethereum.Web3.Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e");
            return FSharpAsync.StartAsTask(
                global::Program.Logic.getCandle(
                    uniswapId, fsharpFunc, TimeSpan.FromSeconds(resolutionSeconds), web3),
                new FSharpOption<TaskCreationOptions>(TaskCreationOptions.None),
                new FSharpOption<CancellationToken>(new CancellationToken()));
        }
    }
}