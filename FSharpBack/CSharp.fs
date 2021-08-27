/// Contains C#-friendly wrappers.
namespace RedDuck.Candleswap.Candles.CSharp

open System
open System.Threading.Tasks
open Microsoft.Extensions.Configuration
open Nethereum.RPC.Eth.DTOs
open Nethereum.Web3;
open RedDuck.Candleswap.Candles
open RedDuck.Candleswap.Candles.Types
open System.Data.SqlClient
open System.Threading
open System.Collections.Generic

type ISqlConnectionProvider =
    abstract GetConnection: unit -> SqlConnection

type SqlConnectionProvider(config: IConfiguration) =
    let connectionString = config.GetSection("ConnectionStrings").["Default"]
    let connection = new SqlConnection(connectionString)
    do connection.Open()
    
    interface ISqlConnectionProvider with
        member _.GetConnection() = connection
    
    interface IDisposable with
        member _.Dispose() =
            connection.Dispose()

type ILogicService =
    abstract CalculateCandlesByTransactions:
        transactionsWithReceipts: struct (Transaction * TransactionReceipt) list ->
        Task<IEnumerable<Pair*Candle>>

    abstract GetCandle:
        callback: Action<string> ->
        resolutionTime: TimeSpan ->
        cancelToken: CancellationToken ->
        Task

    abstract GetCandles:
        callback: Action<string> ->
        resolutionTime: TimeSpan ->
        cancelToken: CancellationToken ->
        startFrom: DateTime -> 
        Task

type LogicService(web3: IWeb3, sqlite: ISqlConnectionProvider) = 
    let toRefTuple = fun struct (a, b) -> (a, b)
    let connection = sqlite.GetConnection()

    interface ILogicService with
        member _.CalculateCandlesByTransactions 
            transactionsWithReceipts
            = 
            List.map toRefTuple transactionsWithReceipts
            |> Logic.calculateCandlesByTransactions connection 
            |> Async.StartAsTask
        
        member _.GetCandle callback resolutionTime cancelToken =
            let callback str = callback.Invoke(str)
            Logic.getCandle connection callback resolutionTime web3 cancelToken |> Async.StartAsTask :> Task

        member _.GetCandles callback resolutionTime cancelToken startFrom= 
            let callback str = callback.Invoke(str)
            Logic.getCandles connection callback resolutionTime web3 cancelToken startFrom |> Async.StartAsTask :> Task

module Logic =
    [<Literal>]
    let MaxUInt256StringRepresentation = "115792089237316195423570985008687907853269984665640564039457584007913129639935"

type ICandleStorageService =
    abstract UpdateCandleAsync: Candle -> Task
    abstract AddCandleAsync: Candle -> Task
    abstract FetchCandlesAsync: 
        pairId: int64 ->
        resolutionSeconds: int -> 
        Task<seq<DbCandle>>
    abstract FetchPairsAsync: unit -> Task<seq<Pair>>
    abstract AddPairAsync: Pair -> Task
    abstract FetchPairAsync: string -> string -> Task<Pair option>
    abstract FetchPairOrCreateNewIfNotExists: string -> string -> Task<Pair>

type CandleStorageService(sql: ISqlConnectionProvider) =
    let connection = sql.GetConnection()
    
    interface ICandleStorageService with
        member _.UpdateCandleAsync candle = 
            Db.updateCandle connection candle |> Async.StartAsTask :> Task
        
        member _.AddCandleAsync candle = 
            Db.addCandle connection candle |> Async.StartAsTask :> Task
        
        member _.FetchCandlesAsync pairId resolutionSeconds = 
            Db.fetchCandles connection pairId resolutionSeconds |> Async.StartAsTask

        member _.FetchPairsAsync () = 
            Db.fetchPairsAsync connection |> Async.StartAsTask

        member _.AddPairAsync pair = 
            Db.addPairAsync connection pair |> Async.StartAsTask :> Task

        member _.FetchPairAsync token0Id token1Id = 
            Db.fetchPairAsync connection token0Id token1Id |> Async.StartAsTask

        member _.FetchPairOrCreateNewIfNotExists token0Id token1Id = 
            Db.fetchPairOrCreateNewIfNotExists connection token0Id token1Id |> Async.StartAsTask

