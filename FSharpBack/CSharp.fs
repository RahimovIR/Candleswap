/// Contains C#-friendly wrappers.
namespace RedDuck.Candleswap.Candles.CSharp

open System
open System.Threading.Tasks
open System.Data.SQLite
open Microsoft.Extensions.Configuration
open Nethereum.RPC.Eth.DTOs
open Nethereum.Web3;
open RedDuck.Candleswap.Candles
open RedDuck.Candleswap.Candles.Types

type ISQLiteConnectionProvider =
    abstract GetConnection: unit -> SQLiteConnection

type SQLiteConnectionProvider(config: IConfiguration) =
    let connectionString = config.GetSection("ConnectionStrings").["Candles"]
    let connection = new SQLiteConnection(connectionString)
    do connection.Open()
    
    interface ISQLiteConnectionProvider with
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
        token0Id: string ->
        token1Id: string ->
        callback: Action<string> ->
        resolutionTime: TimeSpan ->
        unit

type LogicService(web3: IWeb3, sqlite: ISQLiteConnectionProvider) = 
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
        
        member _.GetCandle token0Id token1Id callback resolutionTime =
            let callback str = callback.Invoke(str)
            Logic.getCandle connection token0Id token1Id callback resolutionTime web3

module Logic =
    [<Literal>]
    let MaxUInt256StringRepresentation = "115792089237316195423570985008687907853269984665640564039457584007913129639935"

type ICandleStorageService =
    abstract UpdateCandleAsync: Candle -> Task
    abstract AddCandleAsync: Candle -> Task
    abstract FetchCandlesAsync: 
        token0Id: string -> 
        token1Id: string -> 
        resolutionSeconds: int -> 
        Task<seq<DbCandle>>
    
type CandleStorageService(sqlite: ISQLiteConnectionProvider) =
    let connection = sqlite.GetConnection()
    
    interface ICandleStorageService with
        member _.UpdateCandleAsync candle = 
            Db.updateCandle connection candle |> Async.StartAsTask :> Task
        
        member _.AddCandleAsync candle = 
            Db.addCandle connection candle |> Async.StartAsTask :> Task
        
        member _.FetchCandlesAsync token0Id token1Id resolutionSeconds = 
            Db.fetchCandles connection token0Id token1Id resolutionSeconds |> Async.StartAsTask