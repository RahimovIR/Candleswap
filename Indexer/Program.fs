open System
open Nethereum.Web3
open Nethereum.Hex.HexTypes
open Nethereum.Contracts
open Domain.Types
open Domain.Db
open Microsoft.Data.SqlClient
open Indexer.Logic
open Microsoft.Extensions.Logging

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom

[<EntryPoint>]
let main argv =
    let web3 = Web3 "https://bsc-dataseed.binance.org/"
    let transaction = "0x13b7da1fdfaec5afb3743f7575073ed263ee96885ffa14ee7c491decd82d7676"
                      |> web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync
                      |> Async.AwaitTask
                      |> Async.RunSynchronously

    let receipt = transaction.TransactionHash
                  |> web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync
                  |> Async.AwaitTask
                  |> Async.RunSynchronously

    let logs = receipt.Logs.DecodeAllEvents<SwapEvent>()

    let connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=candleswap;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True")
    connection.Open()

    let logger = new Logger<DbCandle>(new LoggerFactory())

    indexInRangeParallel connection web3 logger 10481429I 10481329I None
    |> Async.RunSynchronously

    while true do ()
    0

