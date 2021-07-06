using Contracts.MystToken.ContractDefinition;
using Contracts.Router.ContractDefinition;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC.Eth.DTOs;
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

            var tokenIn = rnd.Next(0, 10000).ToString();
            var tokenOut = rnd.Next(0, 10000).ToString();
            for (var i = 0; i < tuplesCount; i++)
            {
                var transferEvents = new TransferEventDTO[]
                {
                    new TransferEventDTO() { From = rnd.Next(0, 1000).ToString(), To = rnd.Next(0, 1000).ToString(), Value = rnd.Next(0, 1000000000) },
                    new TransferEventDTO() { From = rnd.Next(0, 1000).ToString(), To = rnd.Next(0, 1000).ToString(), Value = rnd.Next(0, 1000000000) },
                };
                var receipt = new TransactionReceipt { Logs = JArray.FromObject(transferEvents) };
                var inputSingleParams = new ExactInputSingleParams() { TokenIn = tokenIn, TokenOut = tokenOut };
                ExactInputSingleFunction singleFunction = new ExactInputSingleFunction() { Params = inputSingleParams };

                var input = Program.Logic.exactInputSingleId
                    + To32ByteWord(inputSingleParams.TokenIn)
                    + To32ByteWord(inputSingleParams.TokenOut)
                    + To32ByteWord(inputSingleParams.Fee)
                    + To32ByteWord(inputSingleParams.Recipient)
                    + To32ByteWord(inputSingleParams.Deadline)
                    + To32ByteWord(inputSingleParams.AmountIn)
                    + To32ByteWord(inputSingleParams.AmountOutMinimum)
                    + To32ByteWord(inputSingleParams.SqrtPriceLimitX96);

                var transaction = new Transaction() { Input = input };
                transactionsWithReceipts.Add(Tuple.Create(transaction, receipt));
            }
            var computation = Program.Logic.partlyBuildCandle(transactionsWithReceipts.ToArray(), tokenIn, tokenOut,
                new Program.Candle(0, 0, 0, 0, 0), false, false,
                new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e"));
            var cancelBuildCandle = new CancellationTokenSource();
            Console.WriteLine((await FSharpAsync.StartAsTask(computation, 
                new FSharpOption<TaskCreationOptions>(TaskCreationOptions.None), 
                new FSharpOption<CancellationToken>(cancelBuildCandle.Token))).Item1._open);
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
                            str = str.Insert(0, "0");
                        }
                        return str;
                    }

                case BigInteger integer:
                    return To32ByteWord(integer.ToString("X"));
            }
            return To32ByteWord(item.ToString());
        }
    }
}
