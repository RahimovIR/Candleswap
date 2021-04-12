// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Data.GraphQL
open Newtonsoft.Json.Linq
open System.Data.SQLite
open System.Collections
open System.Collections.Generic
open Dapper
open System.Timers

module Requests = 
    
    let top100Query =
        """query q {
               pairs(first: 100, orderBy: reserveUSD, orderDirection: desc) {
                 id
                 token0{
                   symbol
                 }
                 token1{
                   symbol
                 }
               }
              }"""
    
    let swapsQuery id = 
        $"""query q {{
               swaps(orderBy: timestamp, orderDirection: desc, 
                     where:{{ pair: "{id}" }})
                {{
                    amount0In
                    amount0Out
                    amount1In
                    amount1Out
                    timestamp
                }}
               }}"""
  
    let pairInfoQuery id =
           $"""query q {{
               pair(id: "{id}"){{
                   reserve0
                   reserve1
                   token0Price
                   token1Price
               }}
              }}"""
    
    let requestMaker query =
        use connection = new GraphQLClientConnection()
        let request : GraphQLRequest =
            { Query = query
              Variables = [||]
              ServerUrl = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"
              HttpHeaders = [| |]
              OperationName = Some "q" }
        GraphQLClient.sendRequest connection request
    
    type Swap = { id: string; amount0In: float; amount0Out: float; amount1In:float; amount1Out: float; timestamp: int64 } 
    type PairInfo = { reserve0: float; reserve1: float; price0: float; price1: float }
    
    let mapTop100 (token: JToken Option) =
        let mapper (token : JProperty) =
            let strConcat x y = x + "/" + y
            token.Value.["pairs"] |> Seq.map (fun x -> ((x.["id"].ToString()), strConcat (x.["token0"].["symbol"].ToString()) (x.["token1"].["symbol"].ToString())))
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
      
    let mapSwaps (token: JToken Option) =
        let mapper (token : JProperty) =
            token.Value.["swaps"] |> Seq.map (fun x -> { id=(string x.["id"]); amount0In=(float x.["amount0In"]); amount0Out=(float x.["amount0Out"]); amount1In=(float x.["amount1In"]); amount1Out=(float x.["amount1Out"]); timestamp=(int64 x.["timestamp"]);})
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
        
    let mapPairInfo (token: JToken Option) =
        let mapper (token : JProperty) =
            let info = token.Value.["pair"]
            { reserve0 = (float info.["reserve0"]); reserve1 = (float info.["reserve1"]); price0 = (float info.["token0Price"]); price1 = (float info.["token1Price"]) } 
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> Some
        |None -> None
        
    let deserialize (data : string) =
        if String.IsNullOrWhiteSpace(data)
        then None
        else data |> JToken.Parse |> Some
    
    let allPr x = printfn "%A" x
   
    let takeTop100 = top100Query |> requestMaker |> deserialize |> mapTop100
    let takeSwaps idPair = idPair |> swapsQuery |> requestMaker |> deserialize |> mapSwaps
    let takeInfo idPair = idPair |> pairInfoQuery |> requestMaker |> deserialize |> mapPairInfo
    
    //"0xa478c2975ab1ea89e8196811f51a7b7ade33eb11" |> takeSwaps |> allPr
    //takeTop100 |> allPr 

type Candle = { 
    datetime:DateTime; 
    resolutionSeconds:int; 
    uniswapPairId:string;
    _open:decimal;
    high:decimal;
    low:decimal;
    close:decimal;
    volume:decimal;
}

module DB = 
    let private databaseFilename = __SOURCE_DIRECTORY__ + @"\Database\candles.db"
    let private connectionString = sprintf "Data Source=%s;Version=3;" databaseFilename
    let private connection = new SQLiteConnection(connectionString)
    do connection.Open()
   
    let private fetchCandlesSql = @"select datetime, resolutionSeconds, uniswapPairId, open as _open, high, low, close, volume from candles
        where uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds"

    let private insertCandleSql = 
        "insert into candles(datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume) " + 
        "values (@datetime, @resolutionSeconds, @uniswapPairId, @_open, @high, @low, @close, @volume)"
        
    let private updateCandleSql = 
        "update candles set open = @_open, high = @high, low = @low, close = @close, volume = @volume) " + 
        "values uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds and datetime = @datetime)"

    let inline (=>) k v = k, box v

    let private dbQuery<'T> (connection:SQLiteConnection) (sql:string) (parameters:IDictionary<string, obj> option) = 
        match parameters with
        | Some(p) -> connection.QueryAsync<'T>(sql, p)
        | None    -> connection.QueryAsync<'T>(sql)

    let private dbExecute (connection:SQLiteConnection) (sql:string) (data:_) = 
        connection.ExecuteAsync(sql, data)
    
    let fetchCandles (uniswapPairId:string) (resolutionSeconds:int) = 
        async {            
            let! candles = 
                Async.AwaitTask <| 
                dbQuery<Candle> connection fetchCandlesSql 
                    (Some(dict [ "uniswapPairId" => uniswapPairId; "resolutionSeconds" => resolutionSeconds ]))

            return candles
        }
    
    
    let fetchCandlesTask (uniswapPairId:string) (resolutionSeconds:int) = Async.StartAsTask <| fetchCandles uniswapPairId resolutionSeconds
    let addCandle candle = 
        async {
            let queryParams = dict [
                    "datetime" => candle.datetime; 
                    "resolutionSeconds" => candle.resolutionSeconds;
                    "uniswapPairId" => candle.uniswapPairId;
                    "open" => candle._open;
                    "high" => candle.high;
                    "low" => candle.low;
                    "close" => candle.close;
                    "volume" => candle.volume
                    ]

            let! rowsChanged = Async.AwaitTask <| dbExecute connection insertCandleSql candle
            printfn "records added: %i" rowsChanged
        }
    
    let updateCandle candle = 
        async {
            let queryParams = dict [
                    "datetime" => candle.datetime; 
                    "resolutionSeconds" => candle.resolutionSeconds;
                    "uniswapPairId" => candle.uniswapPairId;
                    "open" => candle._open;
                    "high" => candle.high;
                    "low" => candle.low;
                    "close" => candle.close;
                    "volume" => candle.volume
                    ]

            let! rowsChanged = Async.AwaitTask <| dbExecute connection updateCandleSql candle
            printfn "records added: %i" rowsChanged
        }
    
let asyncMain = async {
    let! candles = DB.fetchCandles "0" 0

    printfn "candles 1: %A" candles

    let candle = {
        datetime = DateTime(2004, 03, 28);
        resolutionSeconds = 60;
        uniswapPairId = "tratata";
        _open = 10m;
        high = 11.0555m;
        low = 0.00003m;
        close = 0.0001m;
        volume = 0.0001m;
    }

    do! DB.addCandle candle

    let! candles = DB.fetchCandles candle.uniswapPairId candle.resolutionSeconds

    printfn "candles 2: %A" candles

}

module Logic = 
    let milisecondsInMinute = 60000

    let getSwapsInScope(swaps: Requests.Swap list, timestampAfter: int64, timestampBefore: int64) =
        swaps |> List.filter(fun s -> s.timestamp > timestampAfter && s.timestamp < timestampBefore)

    let countSwapPrice(resBase: decimal, resQuote: decimal) = 
        resQuote / resBase

    let buildCandle (swapsSlice: Requests.Swap list, res0: decimal, res1: decimal, uniswapPairId, datetime:DateTime, resolutionSeconds) = 
        let mutable currentRes0 = res0
        let mutable currentRes1 = res1
        let k = currentRes0 * currentRes1
        let mutable highPrice = 0 |> decimal
        let mutable lowPrice = Decimal.MaxValue
        let mutable openPrice = 0 |> decimal
        let mutable closePrice = currentRes1 / currentRes0
        let mutable volume = 0m
        for s in swapsSlice do
            currentRes1 <- currentRes1 - ((s.amount1In + s.amount1Out) |> decimal)
            currentRes0 <- k / currentRes1
            let currentPrice = currentRes1 / currentRes0
            if (currentPrice > highPrice) then highPrice <- currentPrice
            if (currentPrice < lowPrice) then lowPrice <- currentPrice
            volume <- volume + ((s.amount1In + s.amount1Out) |> decimal)
        openPrice <- currentRes1 / currentRes0
        {
            datetime = datetime;
            resolutionSeconds = resolutionSeconds;
            uniswapPairId = uniswapPairId;
            _open = openPrice;
            high = highPrice;
            low = lowPrice;
            close = closePrice;
            volume = volume;
        }

    let getCandles(pairId: string, callback, resolutionSeconds) = 
        let pair = Requests.takeInfo(pairId)
        let res0 = pair.Value.reserve0 |> decimal
        let res1 = pair.Value.reserve1 |> decimal
        let currentTime = new DateTimeOffset(DateTime.UtcNow)
        let timeMinuteAgo = currentTime.AddSeconds(-resolutionSeconds |> float)
        
        let currentSwaps = (pairId |> Requests.takeSwaps).Value
        let candle = buildCandle(getSwapsInScope(
                                        currentSwaps, 
                                        timeMinuteAgo.ToUnixTimeSeconds(), 
                                        currentTime.ToUnixTimeSeconds()), res0, res1, pairId, DateTime.UtcNow, resolutionSeconds)
        callback candle
        (DB.addCandle >> Async.RunSynchronously) candle
        Threading.Thread.Sleep(1000)



[<EntryPoint>]
let main args =
    //Async.RunSynchronously <| asyncMain
    let id = "0x0d4a11d5eeaac28ec3f61d100daf4d40471f1852"
    let timer = new Timer(20000.0)
    let candlesHandler = new ElapsedEventHandler(fun obj args -> 
                                                 (id, (fun c -> printfn "%A" c), timer.Interval / 1000.0 |> int)
                                                 |> Logic.getCandles |> Requests.allPr)
    timer.Elapsed.AddHandler(candlesHandler)
    timer.Start()
    while true do ()
    0