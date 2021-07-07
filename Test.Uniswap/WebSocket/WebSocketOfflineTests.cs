using Contracts.MystToken.ContractDefinition;
using Contracts.Router.ContractDefinition;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Uniswap
{
    [TestClass]
    public class WebSocketOfflineTests
    {
        [TestMethod]
        public async Task GetCandles_Offline()
        {
            var tuplesCount = 3;
            var rnd = new Random();
            List<Tuple<Transaction, TransactionReceipt>> transactionsWithReceipts = new();

            var tokenIn = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            var tokenOut = To32ByteWord(rnd.Next(0, 10000)).ToLowerInvariant();
            for (var i = 0; i < tuplesCount; i++)
            {
                var transferEvents = new TransferEventDTO[]
                {
                    new TransferEventDTO() 
                    {
                        From = To32ByteWord(rnd.Next(0, 1000)),
                        To = To32ByteWord(rnd.Next(0, 1000)),
                        Value = rnd.Next(0, 1000000000)
                    },
                    new TransferEventDTO() 
                    {
                        From = To32ByteWord(rnd.Next(0, 1000)),
                        To = To32ByteWord(rnd.Next(0, 1000)),
                        Value = rnd.Next(0, 1000000000) 
                    },
                    new TransferEventDTO() 
                    {
                        From = To32ByteWord(rnd.Next(0, 1000)),
                        To = To32ByteWord(rnd.Next(0, 1000)),
                        Value = rnd.Next(1, 1000000000) 
                    },
                    new TransferEventDTO() 
                    {
                        From = To32ByteWord(rnd.Next(0, 1000)),
                        To = To32ByteWord(rnd.Next(0, 1000)),
                        Value = rnd.Next(1, 1000000000) 
                    },
                };

                var transferEventsTokens = new List<JToken>();
                foreach (var transferEvent in transferEvents)
                {
                    transferEventsTokens.Add(new JObject() 
                    { 
                        ["address"] = "0x95ad61b0a150d79219dcf64e1e6cc01f0b64c4ce",
                        ["blockHash"] = "0x6d2d7977bc3ff92363e4a8bc53e98490739c8160edc8a83fc794fd88b85bbb6f",
                        ["blockNumber"] = "0xc300c5",
                        ["data"] = To32ByteWord(transferEvent.Value),
                        ["logIndex"] = "0xab",
                        ["removed"] = false,
                        ["topics"] = new JArray()
                        {
                            "0xddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef",
                            transferEvent.To,
                            transferEvent.From
                        },
                        ["transactionHash"] = "0x84c705dedc17870682b8667b2412b66eab996c651b573bad57c9ab689c168db8",
                        ["transactionIndex"] = "0xb3",
                    });
                }
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(transferEventsTokens) };
                var inputSingleParams = new ExactInputSingleParams() { TokenIn = tokenIn, TokenOut = tokenOut };

                var input = Program.Logic.exactInputSingleId;
                var addParams = inputSingleParams.TokenIn
                    + inputSingleParams.TokenOut
                    + To32ByteWord(inputSingleParams.Fee)
                    + To32ByteWord(inputSingleParams.Recipient)
                    + To32ByteWord(inputSingleParams.Deadline)
                    + To32ByteWord(inputSingleParams.AmountIn)
                    + To32ByteWord(inputSingleParams.AmountOutMinimum)
                    + To32ByteWord(inputSingleParams.SqrtPriceLimitX96);
                addParams = addParams.Replace("0x", string.Empty);
                input += addParams;

                var transaction = new Transaction() { Input = input };
                transactionsWithReceipts.Add(Tuple.Create(transaction, receipt));
            }
            var computation = Program.Logic.partlyBuildCandle(transactionsWithReceipts.ToArray(), 
                tokenIn[26..].Insert(0, "0x"), 
                tokenOut[26..].Insert(0, "0x"),
                new Program.Candle(_open: 0, high: 0, 
                    low: BigDecimal.Parse(Program.Logic.maxUInt256StringRepresentation),
                    close: 0, volume: 0), 
                wasRequiredTransactionsInPeriodOfTime: true, firstIterFlag: true,
                new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e"));
            var cancelBuildCandle = new CancellationTokenSource();
            Program.Candle candle = (await FSharpAsync.StartAsTask(computation,
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
                        if (!str.Contains("0x", StringComparison.CurrentCulture))
                        {
                            str = str.Insert(0, "0x");
                        }
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
