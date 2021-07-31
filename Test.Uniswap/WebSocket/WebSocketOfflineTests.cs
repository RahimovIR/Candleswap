using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Newtonsoft.Json.Linq;
using Contracts.UniswapV3Router.ContractDefinition;
using Contracts.UniswapV2Router.ContractDefinition;
using Contracts.UniswapV1Exchange.ContractDefinition;
using RedDuck.Candleswap.Candles;
using static RedDuck.Candleswap.Candles.Types;
using static RedDuck.Candleswap.Candles.SwapRouterV3;

namespace Test.Uniswap
{
    [TestClass]
    public class WebSocketOfflineTests
    {
        private const int encodedAddressArrayVal = 160; // doesn't appear in SwapExactTokensForTokens signature but comes with response from uniswap

        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV3Candles_Offline(int tuplesCount, int swapsCount)
        {
            InitOfflineTest(@"../../../WebSocket/offlineMockParams.json",
                out Random rnd,
                out string configJson,
                out List<(Transaction, TransactionReceipt)> transactionsWithReceipts);

            var tokenIn = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var swapEvents = new List<SwapEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    swapEvents.Add(GenerateV3SwapEvent(rnd));
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
                transactionsWithReceipts.Add((transaction, receipt));
            }

            // substring from 26th element due to tokens being decoded in 16 bytes, not 32
            await BuildCandleAndAssert(transactionsWithReceipts, tokenIn[26..].Insert(0, "0x"),
                                       tokenOut[26..].Insert(0, "0x"));
        }

        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV2Candles_Offline(int tuplesCount, int swapsCount)
        {
            InitOfflineTest(@"../../../WebSocket/offlineMockParamsV2Router.json",
                out Random rnd,
                out string configJson,
                out List<(Transaction, TransactionReceipt)> transactionsWithReceipts);

            var tokenIn = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var swapEvents = new List<SwapRouterV2.SwapEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    swapEvents.Add(GenerateV2SwapEvent(rnd));
                }

                var swapEventsTokens = new List<JToken>();
                foreach (var swapEvent in swapEvents)
                {
                    var transferJson = JObject.Parse(configJson);
                    transferJson["data"] = To32ByteWord(swapEvent.Amount0In)
                        + (To32ByteWord(swapEvent.Amount1In)
                        + To32ByteWord(swapEvent.Amount0Out)
                        + To32ByteWord(swapEvent.Amount1Out)).Replace("0x", string.Empty);
                    transferJson["topics"] = new JArray()
                    {
                        transferJson["signature"],
                        swapEvent.To,
                        swapEvent.Sender
                    };
                    transferJson.Remove("signature");
                    swapEventsTokens.Add(transferJson);
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(swapEventsTokens) };
                var swapExactParams = new SwapExactTokensForTokensFunction()
                {
                    AmountIn = rnd.Next(1, 1000),
                    AmountOutMin = rnd.Next(1, 1000),
                    To = To32ByteWord(rnd.Next(1, 10000)).ToLowerInvariant(),
                    Deadline = rnd.Next(1, 1000),
                    Path = new List<string>() { tokenIn, tokenOut }
                };

                var input = SwapRouterV2.swapExactTokensForTokensId;
                var addParams = To32ByteWord(swapExactParams.AmountIn)
                    + To32ByteWord(swapExactParams.AmountOutMin)
                    + To32ByteWord(encodedAddressArrayVal)
                    + To32ByteWord(swapExactParams.To)
                    + To32ByteWord(swapExactParams.Deadline)
                    + To32ByteWord(swapExactParams.Path);
                input += addParams.Replace("0x", string.Empty);

                var transaction = new Transaction() { Input = input, To = SwapRouterV2.router02Address };
                transactionsWithReceipts.Add((transaction, receipt));
            }
            await BuildCandleAndAssert(transactionsWithReceipts, tokenIn[26..].Insert(0, "0x"),
                                       tokenOut[26..].Insert(0, "0x"));
        }

        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV1Candles_TransferEvent_Offline(int tuplesCount, int swapsCount)
        {
            InitOfflineTest(@"../../../WebSocket/offlineMockParamsV1Router.json",
                out Random rnd,
                out string configJson,
                out List<(Transaction, TransactionReceipt)> transactionsWithReceipts);

            var tokenIn = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var swapEvents = new List<TransferEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    swapEvents.AddRange(GenerateV1TransferEvents(rnd, tokenIn, tokenOut));
                }

                var swapEventsTokens = new List<JToken>();
                foreach (var swapEvent in swapEvents)
                {
                    var transferJson = JObject.Parse(configJson);
                    transferJson["data"] = To32ByteWord(swapEvent.Value);
                    transferJson["topics"] = new JArray()
                    {
                        transferJson["signature"],
                        swapEvent.From,
                        swapEvent.To
                    };
                    transferJson.Remove("signature");
                    swapEventsTokens.Add(transferJson);
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(swapEventsTokens) };
                var swapExactParams = new TokenToTokenSwapOutputFunction()
                {
                    Tokens_bought = rnd.Next(1, 100000),
                    Max_tokens_sold = rnd.Next(1, 100000),
                    Max_eth_sold = rnd.Next(1, 100000),
                    Deadline = rnd.Next(1, 100000),
                    Token_addr = To32ByteWord(rnd.Next(1, 100000)).ToLowerInvariant()
                };

                var input = ExchangeV1.tokenToTokenSwapOutputId;
                var addParams = To32ByteWord(swapExactParams.Tokens_bought)
                    + To32ByteWord(swapExactParams.Max_tokens_sold)
                    + To32ByteWord(swapExactParams.Max_eth_sold)
                    + To32ByteWord(swapExactParams.Deadline)
                    + swapExactParams.Token_addr;
                input += addParams.Replace("0x", string.Empty);

                var transaction = new Transaction() { Input = input, To = ExchangeV1.exchangeAddress };
                transactionsWithReceipts.Add((transaction, receipt));
            }
            await BuildCandleAndAssert(transactionsWithReceipts, tokenIn[26..].Insert(0, "0x"),
                                       tokenOut[26..].Insert(0, "0x"));
        }

        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV1Candles_EthPurchaseEvent_Offline(int tuplesCount, int swapsCount)
        {
            InitOfflineTest(@"../../../WebSocket/offlineMockParamsV1Router_EthPurchaseEvent.json", 
                out Random rnd, 
                out string configJson,
                out List<(Transaction, TransactionReceipt)> transactionsWithReceipts);

            var tokenIn = ExchangeV1.wethAddress;
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var purchaseEvents = new List<ExchangeV1.EthPurchaseEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    purchaseEvents.Add(GenerateV1EthPurchaseEvent(rnd, tokenOut));
                }

                var swapEventsTokens = new List<JToken>();
                foreach (var purchaseEvent in purchaseEvents)
                {
                    var transferJson = JObject.Parse(configJson);
                    transferJson["data"] = string.Empty;
                    transferJson["topics"] = new JArray()
                    {
                        transferJson["signature"],
                        purchaseEvent.Buyer,
                        purchaseEvent.TokensSold,
                        purchaseEvent.EthBought
                    };
                    transferJson.Remove("signature");
                    swapEventsTokens.Add(transferJson);
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(swapEventsTokens) };
                var swapExactParams = new EthToTokenSwapOutputFunction()
                {
                    Tokens_bought = rnd.Next(1, 100000),
                    Deadline = rnd.Next(1, 100000),
                };

                var input = ExchangeV1.ethToTokenSwapOutputId;
                var addParams = To32ByteWord(swapExactParams.Tokens_bought)
                    + To32ByteWord(swapExactParams.Deadline);
                input += addParams.Replace("0x", string.Empty);

                var transaction = new Transaction() { Input = input, To = ExchangeV1.exchangeAddress };
                transactionsWithReceipts.Add((transaction, receipt));
            }
            await BuildCandleAndAssert(transactionsWithReceipts, tokenIn,
                                       tokenOut[26..].Insert(0, "0x"));
        }
        
        [TestMethod]
        [DataRow(3, 1)]
        public async Task GetV1Candles_TokenPurchaseEvent_Offline(int tuplesCount, int swapsCount)
        {
            InitOfflineTest(@"../../../WebSocket/offlineMockParamsV1Router_EthPurchaseEvent.json", 
                out Random rnd, 
                out string configJson,
                out List<(Transaction, TransactionReceipt)> transactionsWithReceipts);

            var tokenIn = ExchangeV1.wethAddress;
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var purchaseEvents = new List<ExchangeV1.TokenPurchaseEventDTO>();
                for (int j = 0; j < swapsCount; j++)
                {
                    purchaseEvents.Add(GenerateV1TokenPurchaseEvent(rnd, tokenOut));
                }

                var swapEventsTokens = new List<JToken>();
                foreach (var purchaseEvent in purchaseEvents)
                {
                    var transferJson = JObject.Parse(configJson);
                    transferJson["data"] = string.Empty;
                    transferJson["topics"] = new JArray()
                    {
                        transferJson["signature"],
                        purchaseEvent.Buyer,
                        purchaseEvent.TokensBought,
                        purchaseEvent.EthSold
                    };
                    transferJson.Remove("signature");
                    swapEventsTokens.Add(transferJson);
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(swapEventsTokens) };
                var swapExactParams = new TokenToEthSwapOutputFunction()
                {
                    Eth_bought = rnd.Next(1, 100000),
                    Deadline = rnd.Next(1, 100000),
                };

                var input = ExchangeV1.ethToTokenSwapOutputId;
                var addParams = To32ByteWord(swapExactParams.Eth_bought)
                    + To32ByteWord(swapExactParams.Deadline);
                input += addParams.Replace("0x", string.Empty);

                var transaction = new Transaction() { Input = input, To = ExchangeV1.exchangeAddress };
                transactionsWithReceipts.Add((transaction, receipt));
            }
            await BuildCandleAndAssert(transactionsWithReceipts, tokenIn,
                                       tokenOut[26..].Insert(0, "0x"));
        }

        private static void InitOfflineTest(string configJsonAddress, out Random rnd, out string configJson,
                                            out List<(Transaction, TransactionReceipt)> transactionsWithReceipts)
        {
            rnd = new Random();
            using (var reader = new StreamReader(configJsonAddress))
            {
                configJson = reader.ReadToEnd();
            }
            transactionsWithReceipts = new();
        }

        private static Task BuildCandleAndAssert(
            List<(Transaction, TransactionReceipt)> transactionsWithReceipts,
            string tokenIn, string tokenOut)
        {
            var computation =
                Logic.partlyBuildCandle(
                    transactionsWithReceipts.Select(x => Tuple.Create(x.Item1, x.Item2)).ToArray(),
                    tokenIn,
                    tokenOut,
                    new Candle(_open: 0, high: 0,
                        low: BigDecimal.Parse(RedDuck.Candleswap.Candles.CSharp.Logic.MaxUInt256StringRepresentation),
                        close: 0, volume: 0),
                    wasRequiredTransactionsInPeriodOfTime: true, firstIterFlag: true);
            Candle candle = computation.Item1;
            Console.WriteLine(candle);
            Assert.IsNotNull(candle);
            Assert.IsTrue(candle._open != 0);
            Assert.IsTrue(candle.high != 0);
            Assert.IsTrue(candle.low != 0);
            Assert.IsTrue(candle.close != 0);
            Assert.IsTrue(candle.volume != 0);
            
            return Task.CompletedTask;
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
                    return To32ByteWord(integer.ToString("X").ToLowerInvariant());
                case int integer:
                    return To32ByteWord(integer.ToString("X").ToLowerInvariant());
                case IEnumerable<object> enumerable:
                    var resStr = string.Empty;
                    resStr += To32ByteWord(enumerable.Count());
                    foreach (var member in enumerable)
                    {
                        resStr += To32ByteWord(member);
                    }
                    return resStr;
            }
            return To32ByteWord(item.ToString());
        }

        private TransferEventDTO[] GenerateV1TransferEvents(Random rnd, string tokenIn, string tokenOut)
        {
            return new[]
            {
                new TransferEventDTO
                {
                    From = tokenIn,
                    To = To32ByteWord(rnd.Next(1, 1000)).ToLowerInvariant(),
                    Value = rnd.Next(1, 1000)
                },
                new TransferEventDTO
                {
                    From = tokenOut,
                    To = To32ByteWord(rnd.Next(1, 1000)).ToLowerInvariant(),
                    Value = rnd.Next(1, 1000)
                }
            };
        }

        private ExchangeV1.TokenPurchaseEventDTO GenerateV1TokenPurchaseEvent(Random rnd, string tokenOut)
        {
            return new ExchangeV1.TokenPurchaseEventDTO
            {
                Buyer = tokenOut,
                EthSold = rnd.Next(1, 1000),
                TokensBought = rnd.Next(1, 1000)
            };
        }

        private ExchangeV1.EthPurchaseEventDTO GenerateV1EthPurchaseEvent(Random rnd, string tokenOut)
        {
            return new ExchangeV1.EthPurchaseEventDTO
            {
                Buyer = tokenOut,
                EthBought = rnd.Next(1, 1000),
                TokensSold = rnd.Next(1, 1000)
            };
        }

        private SwapRouterV2.SwapEventDTO GenerateV2SwapEvent(Random rnd)
        {
            return new SwapRouterV2.SwapEventDTO()
            {
                Amount0In = 0,
                Amount1In = rnd.Next(1, 10000),
                Amount0Out = rnd.Next(1, 10000),
                Amount1Out = 0,
                Sender = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant(),
                To = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant()
            };
        }

        private SwapEventDTO GenerateV3SwapEvent(Random rnd)
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
    }
}
