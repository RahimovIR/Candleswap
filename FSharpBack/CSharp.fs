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
    abstract PartlyBuildCandle:
        transactionsWithReceipts: struct (Transaction * TransactionReceipt) [] ->
        token0Id: string ->
        token1Id: string ->
        candle: Candle ->
        wasRequiredTransactionsInPeriodOfTime: bool ->
        firstIterFlag: bool ->
        Candle * bool * bool

    abstract GetCandle:
        pair: Pair ->
        callback: Action<string> ->
        resolutionTime: TimeSpan ->
        cancelToken: CancellationToken ->
        unit

    abstract GetCandles:
        pair: Pair ->
        callback: Action<string> ->
        resolutionTime: TimeSpan ->
        cancelToken: CancellationToken ->
        unit

type LogicService(web3: IWeb3, sqlite: ISqlConnectionProvider) = 
    let toRefTuple = fun struct (a, b) -> (a, b)
    let connection = sqlite.GetConnection()

    interface ILogicService with
        member _.PartlyBuildCandle 
            transactionsWithReceipts
            token0Id
            token1Id
            candle
            wasRequiredTransactionsInPeriodOfTime
            firstIterFlag = 
            Logic.partlyBuildCandle 
                (Array.map toRefTuple transactionsWithReceipts) 
                token0Id
                token1Id
                candle
                wasRequiredTransactionsInPeriodOfTime
                firstIterFlag
        
        member _.GetCandle pair callback resolutionTime cancelToken =
            let callback str = callback.Invoke(str)
            Logic.getCandle connection pair callback resolutionTime web3 cancelToken

        member _.GetCandles pair callback resolutionTime cancelToken = 
            let callback str = callback.Invoke(str)
            Logic.getCandles connection pair callback resolutionTime web3 cancelToken

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

type CandleStorageService(sqlite: ISqlConnectionProvider) =
    let connection = sqlite.GetConnection()
    
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