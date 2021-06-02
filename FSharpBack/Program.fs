// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open FSharp.Data.GraphQL
open Newtonsoft.Json.Linq
open System.Data.SQLite
open System.Collections
open System.Collections.Generic
open Dapper
open System.Timers
open Nethereum.Web3
open Nethereum.Hex.HexTypes
open Nethereum.RPC.Eth.DTOs
open System.Numerics

module Requests = 
    
    let pairsQuery =
        """query q {
               pairs(orderBy: reserveUSD, orderDirection: desc) {
                   token0{
                       id
                   }
                   token1{
                       id
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
    type Pair = {token0Id: string; token1Id:string}
    
    let mapPairs (token: JToken Option) =
        let mapper (token : JProperty) =
            token.Value.["pairs"] |> Seq.map (fun x -> {token0Id = x.["token0"].["id"].ToString(); token1Id = x.["token1"].["id"].ToString()})
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
   
    let takePairs () = pairsQuery |> requestMaker |> deserialize |> mapPairs
    let takeSwaps idPair = idPair |> swapsQuery |> requestMaker |> deserialize |> mapSwaps
    let takeInfo idPair = idPair |> pairInfoQuery |> requestMaker |> deserialize |> mapPairInfo

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

    let private getCandleByDatetimeSql =
        "select datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume" + 
        "from candles" +
        "where datetime = @datetime"

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
            let! rowsChanged = Async.AwaitTask <| dbExecute connection insertCandleSql candle
            printfn "records added: %i" rowsChanged
        }
    
    let updateCandle candle = 
        async {
            let! rowsChanged = Async.AwaitTask <| dbExecute connection updateCandleSql candle
            printfn "records added: %i" rowsChanged
        }

    let getCandleByDatetime datetime = 
        async {
            let queryParams = Some <| dict [
                    "datetime" => datetime;
                    ]

            let! candle = Async.AwaitTask <| dbQuery connection getCandleByDatetimeSql queryParams
            return candle
        }


module Logic = 
    let milisecondsInMinute = 60000

    let getSwapsInScope(swaps: Requests.Swap list, timestampAfter: int64, timestampBefore: int64) =
        swaps |> List.filter(fun s -> s.timestamp >= timestampAfter && s.timestamp <= timestampBefore)

    let countSwapPrice(resBase: decimal, resQuote: decimal) = 
        resQuote / resBase

    let buildCandle (swapsSlice: Requests.Swap list, res0: decimal, res1: decimal, uniswapPairId, datetime:DateTime, resolutionSeconds):Candle option = 
        let mutable currentRes0 = res0
        let mutable currentRes1 = res1
        let k = currentRes0 * currentRes1
        let mutable closePrice = currentRes1 / currentRes0
        let mutable lowPrice = closePrice
        let mutable highPrice = closePrice
        let mutable volume = 0m
        for s in swapsSlice do
            currentRes1 <- currentRes1 - ((s.amount1In + s.amount1Out) |> decimal)
            currentRes0 <- k / currentRes1
            let currentPrice = currentRes1 / currentRes0
            if (currentPrice > highPrice) then highPrice <- currentPrice
            if (currentPrice < lowPrice) then lowPrice <- currentPrice
            volume <- volume + ((s.amount1In + s.amount1Out) |> decimal)
        let openPrice = currentRes1 / currentRes0 
        match swapsSlice.Length with
        | 0 -> None
        | _ -> Some {
                        datetime = datetime;
                        resolutionSeconds = resolutionSeconds;
                        uniswapPairId = uniswapPairId;
                        _open = openPrice;
                        high = highPrice;
                        low = lowPrice;
                        close = closePrice;
                        volume = volume;
                    }

    let getCandle(pairId: string, callback, resolutionTime:TimeSpan) = 
        let pair = Requests.takeInfo(pairId)
        let res0 = pair.Value.reserve0 |> decimal
        let res1 = pair.Value.reserve1 |> decimal
        let resolutionSeconds = (int)resolutionTime.TotalSeconds
        let currentTime = new DateTimeOffset(DateTime.UtcNow)
        let resolutionTimeAgo = currentTime.Subtract(resolutionTime)
        
        
        let currentSwaps = (pairId |> Requests.takeSwaps).Value
        let candle = buildCandle(getSwapsInScope(
                                        currentSwaps, 
                                        resolutionTimeAgo.ToUnixTimeSeconds(), 
                                        currentTime.ToUnixTimeSeconds()), res0, res1, pairId, resolutionTimeAgo.DateTime, resolutionSeconds)
        match candle with
        | Some candle -> callback ($"uniswapPairId:{candle.uniswapPairId}\nresolutionSeconds:{candle.resolutionSeconds}\n"+
                                   $"datetime:{candle.datetime}\n_open:{candle._open}\nlow:{candle.low}\nhigh:{candle.high}\n"+
                                   $"close:{candle.close}\nvolume:{candle.volume}")
                         (DB.addCandle >> Async.RunSynchronously) candle
        | None -> callback $"No swaps\nfrom:{resolutionTimeAgo.DateTime}\nto:{currentTime.DateTime}"
        Threading.Thread.Sleep(1000)

    let getCandles(pairId: string, callback, resolutionTime:TimeSpan) = 
        let pair = Requests.takeInfo(pairId)
        let res0 = pair.Value.reserve0 |> decimal
        let res1 = pair.Value.reserve1 |> decimal
        let resolutionSeconds = (int)resolutionTime.TotalSeconds
        let mutable currentTime = new DateTimeOffset(DateTime.UtcNow)
        let mutable resolutionTimeAgo = currentTime.Subtract(resolutionTime)

        while true do
        let currentSwaps = (pairId |> Requests.takeSwaps).Value
        let candle = buildCandle(getSwapsInScope(
                                        currentSwaps, 
                                        resolutionTimeAgo.ToUnixTimeSeconds(), 
                                        currentTime.ToUnixTimeSeconds()), res0, res1, pairId, resolutionTimeAgo.DateTime, resolutionSeconds)
        match candle with
        | Some candle -> callback ($"uniswapPairId:{candle.uniswapPairId}\nresolutionSeconds:{candle.resolutionSeconds}\n"+
                                  $"datetime:{candle.datetime}\n_open:{candle._open}\nlow:{candle.low}\nhigh:{candle.high}\n"+
                                  $"close:{candle.close}\nvolume:{candle.volume}")
                         (DB.addCandle >> Async.RunSynchronously) candle
        | None -> callback $"No swaps\nfrom:{resolutionTimeAgo.DateTime}\nto:{currentTime.DateTime}"
        currentTime <- resolutionTimeAgo
        resolutionTimeAgo <- currentTime.Subtract(resolutionTime)
        Threading.Thread.Sleep(1000)
        ()

module Dater =

    type BlockNumberTimestamp = {
        number:HexBigInteger;
        timestamp:HexBigInteger;
    }
    
    let getBlockNumberAndTimestampAsync (savedBlocks:Dictionary<HexBigInteger, HexBigInteger>) (web3: Web3) (blockNumber:HexBigInteger) = 
        async {
            let timestamp = ref (HexBigInteger "0")
            if savedBlocks.TryGetValue(blockNumber, timestamp)
            then return { number = blockNumber; timestamp = timestamp.Value}
            else let! block =  Async.AwaitTask <|web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber)
                 savedBlocks.Add(blockNumber, block.Timestamp)
                 return {number = blockNumber; timestamp = block.Timestamp}
        }

    let isBestBlock date predictedBlockTime previousBlockTime nextBlockTime ifBlockAfterDate = 
        match ifBlockAfterDate with
        | true -> if predictedBlockTime < date then false
                  else if predictedBlockTime >= date && previousBlockTime < date then true
                  else false
        | false -> if predictedBlockTime > date then false
                   else if predictedBlockTime <= date && nextBlockTime > date then true
                   else false

    let getNextPredictedBlockNumber currentPredictedBlockNumber skip = 
        let nextPredictedBlockNumber = currentPredictedBlockNumber + skip
        if nextPredictedBlockNumber <= 1I then 1I
        else nextPredictedBlockNumber

    let rec findBestBlock date predictedBlock ifBlockAfterDate
                          blockTime savedBlocks checkedBlocks web3 =
        async {
            let! previousPredictedBlock = predictedBlock.number.Value - 1I
                                          |> HexBigInteger
                                          |> getBlockNumberAndTimestampAsync savedBlocks web3
            let! nextPredictedBlock = predictedBlock.number.Value + 1I
                                      |> HexBigInteger
                                      |> getBlockNumberAndTimestampAsync savedBlocks web3

            if isBestBlock date predictedBlock.timestamp.Value previousPredictedBlock.timestamp.Value 
                             nextPredictedBlock.timestamp.Value ifBlockAfterDate
            then return predictedBlock
            else let difference = date - predictedBlock.timestamp.Value
                 let mutable skip = (float difference) / blockTime
                                    |> Math.Ceiling
                 if skip = 0.0 then skip <- if difference < 0I then -1.0 else 1.0
                 let! nextPredictedBlock = getNextPredictedBlockNumber predictedBlock.number.Value (BigInteger skip)
                                           |> HexBigInteger
                                           |> getBlockNumberAndTimestampAsync savedBlocks web3
                 let newBlockTime = (predictedBlock.timestamp.Value - nextPredictedBlock.timestamp.Value)
                                    / (predictedBlock.number.Value - nextPredictedBlock.number.Value)
                                    |> float
                                    |> Math.Abs
                 return! findBestBlock date nextPredictedBlock ifBlockAfterDate newBlockTime savedBlocks checkedBlocks web3                                           
        }

    let getBlockByDateAsync ifBlockAfterDate (web3: Web3) date =
        async {
            let savedBlocks = new Dictionary<HexBigInteger, HexBigInteger>()
            let checkedBlocks = new List<BigInteger>()
            let! latestBlockNumber = web3.Eth.Blocks.GetBlockNumber.SendRequestAsync() |> Async.AwaitTask
            let! latestBlock =  getBlockNumberAndTimestampAsync savedBlocks web3 latestBlockNumber
            let firstBlockNumber = HexBigInteger 1I
            let! firstBlock = getBlockNumberAndTimestampAsync savedBlocks web3 firstBlockNumber
            let blockTime = (float (latestBlock.timestamp.Value - firstBlock.timestamp.Value))
                            / (float (latestBlock.number.Value - 1I))
            
            if date <= firstBlock.timestamp.Value then return firstBlock
            else if date >= latestBlock.timestamp.Value then return latestBlock
            else let! predictedBlock = (float (date - firstBlock.timestamp.Value)) / blockTime
                                       |> Math.Ceiling
                                       |> BigInteger
                                       |> HexBigInteger
                                       |> getBlockNumberAndTimestampAsync savedBlocks web3
                 return! findBestBlock date predictedBlock ifBlockAfterDate blockTime savedBlocks checkedBlocks web3
        }

    let convertToUnixTimestamp (date:DateTime) =
        let origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
        let diff = date.ToUniversalTime() - origin
        Math.Floor(diff.TotalSeconds)


[<EntryPoint>]
let main args =
    (*let id = "0x1fbf001792e8cc747a5cb4aedf8c26b7421147e7"
    let resolutionTime = new TimeSpan(0, 0, 10)
    let timer = new Timer(resolutionTime.TotalMilliseconds)
    let candlesHandler = new ElapsedEventHandler(fun obj args -> 
                                                 (id, (fun c -> printfn "%A" c), resolutionTime)
                                                 |> Logic.getCandle |> Requests.allPr)
    timer.Elapsed.AddHandler(candlesHandler)
    timer.Start()
    while true do ()*)
    (*let id = "0x1fbf001792e8cc747a5cb4aedf8c26b7421147e7"
    let resolutionTime = new TimeSpan(1, 1, 30)
    (id, (fun c -> printfn "%A" c), resolutionTime) |> Logic.getCandles |> Requests.allPr*)
    (*let web3 = new Web3("https://still-dark-waterfall.quiknode.pro/9996011103848e330184be31732e5fa9d14de55a/")
    let date = new DateTime(2020, 10, 20, 13, 20, 41)

    let bl = date
             |> Dater.convertToUnixTimestamp
             |> BigInteger
             |> Dater.getBlockByDateAsync true web3
             |> Async.RunSynchronously
    async{
        let! block = Async.AwaitTask <| web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(bl.number)
        Array.iter (fun transaction -> printfn "%A" transaction.) block.Transactions
        
    } |> Async.RunSynchronously
    printfn "%A" (Dater.convertToUnixTimestamp date)
    printfn "%A" bl.number.Value*)
    let t = Requests.takePairs()
    printf "%A" t
    0