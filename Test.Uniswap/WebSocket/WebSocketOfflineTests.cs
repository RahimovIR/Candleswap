using Contracts.UniswapV3Router.ContractDefinition;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FSharpBack.Logic.SwapRouterV3;
using static FSharpBack.Logic;

namespace Test.Uniswap
{
    [TestClass]
    public class WebSocketOfflineTests
    {
        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV3Candles_Offline(int tuplesCount, int swapsCount)
        {
            var rnd = new Random();
            string configJson;
            using (var reader = new StreamReader(@"../../../WebSocket/offlineMockParams.json"))
            {
                configJson = reader.ReadToEnd();
            }
            List<Tuple<Transaction, TransactionReceipt>> transactionsWithReceipts = new();

            var tokenIn = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var swapEvents = new List<SwapEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    swapEvents.Add(GenerateSwapEvent(rnd));
                }

                var swapEventsTokens = new List<JToken>();
                foreach (var swapEvent in swapEvents)
                {
                    var transferJson = JObject.Parse(configJson);
                    transferJson["data"] = To32ByteWord(swapEvent.Amount0)
                        + (To32ByteWord(swapEvent.Amount1)
                        + To32ByteWord(swapEvent.SqrtPriceX96)
                        + To32ByteWord(swapEvent.Liquidity)
                        + To32ByteWord(swapEvent.Tick)).Replace("0x", string.Empty);
                    transferJson["topics"] = new JArray()
                    {
                        transferJson["signature"],
                        swapEvent.Recipient,
                        swapEvent.Sender
                    };
                    transferJson.Remove("signature");
                    swapEventsTokens.Add(transferJson);
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(swapEventsTokens) };
                var inputSingleParams = new ExactInputSingleParams() { TokenIn = tokenIn, TokenOut = tokenOut };

                var input = exactInputSingleId;
                var addParams = inputSingleParams.TokenIn
                    + inputSingleParams.TokenOut
                    + To32ByteWord(inputSingleParams.Fee)
                    + To32ByteWord(inputSingleParams.Recipient)
                    + To32ByteWord(inputSingleParams.Deadline)
                    + To32ByteWord(inputSingleParams.AmountIn)
                    + To32ByteWord(inputSingleParams.AmountOutMinimum)
                    + To32ByteWord(inputSingleParams.SqrtPriceLimitX96);
                input += addParams.Replace("0x", string.Empty);

                var transaction = new Transaction() { Input = input, To = routerAddress };
                transactionsWithReceipts.Add(Tuple.Create(transaction, receipt));
            }
            var computation = partlyBuildCandle(transactionsWithReceipts.ToArray(), 
                // substring from 26th element due to tokens being decoded in 16 bytes, not 32
                tokenIn[26..].Insert(0, "0x"), 
                tokenOut[26..].Insert(0, "0x"),
                new FSharpBack.Candle(_open: 0, high: 0, 
                    low: BigDecimal.Parse(maxUInt256StringRepresentation),
                    close: 0, volume: 0), 
                wasRequiredTransactionsInPeriodOfTime: true, firstIterFlag: true);
            var cancelBuildCandle = new CancellationTokenSource();
            FSharpBack.Candle candle = (await FSharpAsync.StartAsTask(computation,
                            new FSharpOption<TaskCreationOptions>(TaskCreationOptions.None),
                            new FSharpOption<CancellationToken>(cancelBuildCandle.Token))).Item1;
            Console.WriteLine(candle);
            Assert.IsNotNull(candle);
            Assert.IsTrue(candle._open != 0);
            Assert.IsTrue(candle.high != 0);
            Assert.IsTrue(candle.low != 0);
            Assert.IsTrue(candle.close != 0);
            Assert.IsTrue(candle.volume != 0);
        }

        private SwapEventDTO GenerateSwapEvent(Random rnd)
        {
            return new SwapEventDTO()
            {
                Amount0 = rnd.Next(-10000, 0),
                Amount1 = rnd.Next(0, 10000),
                Sender = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant(),
                Recipient = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant(),
                SqrtPriceX96 = rnd.Next(0, 10000),
                Liquidity = rnd.Next(0, 10000000),
                Tick = rnd.Next(-10000, 0)
            };
        }

        public string To32ByteWord(object item)
        {
            if (item == null)
            {
                return To32ByteWord(string.Empty);
            }
            switch (item)
            {
                case string str:
                    {
                        while (str.Length < 64)
                        {
                            str = str.Insert(!str.Contains("0x", StringComparison.CurrentCulture)
                                    ? 0
                                    : str.IndexOf("0x") + 2, "0");
                            
                        }
                        str = str.Insert(0, "0x");
                        return str;
                    }

                case BigInteger integer:
                    return To32ByteWord(integer.ToString("X"));
                case int integer:
                    return To32ByteWord(integer.ToString("X"));
            }
            return To32ByteWord(item.ToString());
        }
    }
}
