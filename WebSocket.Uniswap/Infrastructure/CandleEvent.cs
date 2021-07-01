using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace WebSocket.Uniswap.Infrastructure
{
    public static class CandleEvent
    {
        public static event Action<string> candles = _ => { };

        public static SortedSet<(string, int)> EventsInvoked = new SortedSet<(string, int)>();

        public static Task GetCandles(string uniswapId, int resolutionSeconds)
        {
            if(EventsInvoked.TryGetValue((uniswapId, resolutionSeconds), out _))
            {
                return Task.CompletedTask;
            }
            else
            {
                EventsInvoked.Add((uniswapId, resolutionSeconds));
            }
            uniswapId = "0xb4e16d0168e52d35cacd2c6185b44281ec28c9dc";
            var fsharpFunc = FuncConvert.ToFSharpFunc<string>(t=>candles(t));
            var web3 = new Nethereum.Web3.Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e");
            var backgroundJob = Task.Run( () => global::Program.Logic.getCandles(uniswapId, fsharpFunc, TimeSpan.FromSeconds(resolutionSeconds), web3));
            return backgroundJob;
        }
    }
}