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
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Hex.HexConvertors.Extensions
open Nethereum.Contracts
open Contracts.Router.ContractDefinition
open Nethereum.Util



module Requests =    
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
                   token0{{
                       id
                   }}
                   token1{{
                       id
                   }}
               }}
              }}"""

    let poolInfoQuery id = 
        $"""query q {{
            pool(id: "{id}"){{
                token0{{
                    id
                }}
                token1{{
                    id
                }}
            }}
           }}"""

    
    let requestMaker serverUrl query =
        use connection = new GraphQLClientConnection()
        let request : GraphQLRequest =
            { Query = query
              Variables = [||]
              ServerUrl = serverUrl
              HttpHeaders = [| |]
              OperationName = Some "q" }
        GraphQLClient.sendRequest connection request
    
    type Swap = { id: string; amount0In: float; amount0Out: float; amount1In:float; amount1Out: float; timestamp: int64 } 
    type PairInfo = { reserve0: BigInteger; reserve1: BigInteger; price0: float; price1: float; token0Id: string; token1Id:string }
    type PoolInfo = {token0Id: string; token1Id: string;}
      
    let mapSwaps (token: JToken Option) =
        let mapper (token : JProperty) =
            token.Value.["swaps"] |> Seq.map (fun x -> { id=(string x.["id"]); amount0In=(float x.["amount0In"]); amount0Out=(float x.["amount0Out"]); amount1In=(float x.["amount1In"]); amount1Out=(float x.["amount1Out"]); timestamp=(int64 x.["timestamp"]);})
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> List.ofSeq |> Some
        |None -> None
        
    let mapPairInfo (token: JToken Option) =
        let mapper (token : JProperty) =
            let info = token.Value.["pair"]
            { 
                reserve0 = (info.Value<decimal>("reserve0") |> BigInteger);
                reserve1 = (info.Value<decimal>("reserve1") |> BigInteger); 
                price0 = (float info.["token0Price"]); 
                price1 = (float info.["token1Price"]);
                token0Id = info.["token0"].["id"].ToString();
                token1Id = info.["token1"].["id"].ToString();
            } 
        match token with
        |Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> Some
        |None -> None

    let mapPoolInfo (token: JToken Option) =
        let mapper (token: JProperty) = 
            let info = token.Value.["pool"]
            {
                token0Id = info.["token0"].["id"].ToString();
                token1Id = info.["token1"].["id"].ToString();
            }
        match token with
        | Some token -> token.Children<JProperty>() |> Seq.last |> mapper |> Some
        | None -> None
        
    let deserialize (data : string) =
        if String.IsNullOrWhiteSpace(data)
        then None
        else data |> JToken.Parse |> Some
    
    let allPr x = printfn "%A" x

    let uniswapV2 = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"
    let uniswapV3 = "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v3"

    let takeSwaps idPair = idPair |> swapsQuery |> requestMaker uniswapV2 |> deserialize |> mapSwaps
    let takePairInfo idPair = idPair |> pairInfoQuery |> requestMaker uniswapV2 |> deserialize |> mapPairInfo
    let takePoolInfo idPair = idPair |> poolInfoQuery |> requestMaker uniswapV3 |> deserialize |> mapPoolInfo

type Candle = { 
    _open:BigDecimal;
    high:BigDecimal;
    low:BigDecimal;
    close:BigDecimal;
    volume:uint;
}

[<CLIMutable>]
type DBCandle = {
    datetime:DateTime; 
    resolutionSeconds:int; 
    uniswapPairId:string;
    _open:string;
    high:string;
    low:string;
    close:string;
    volume:uint;
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
                dbQuery<DBCandle> connection fetchCandlesSql 
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


module Logic = 
    let milisecondsInMinute = 60000
    let routerAddress = "0xe592427a0aece92de3edee1f18e0157c05861564"
    let maxUInt256StringRepresentation = "115792089237316195423570985008687907853269984665640564039457584007913129639935"
    let exactInputSingleId = "0x414bf389"
    let exactOutputSingleId = "0xdb3e2198"
    let exactInputId = "0xc04b8d59"
    let exactOutputId = "0xf28c0498"
    let multicallId = "0xac9650d8"
    let lengthForSimpleCall = 648
    let lengthForSingleCall = 520
    

    let filterSwapsInScope(swaps: Requests.Swap list, timestampAfter: int64, timestampBefore: int64) =
        swaps |> List.filter (fun s -> s.timestamp >= timestampAfter && s.timestamp <= timestampBefore)

    let countSwapPrice(resBase: decimal, resQuote: decimal) = 
        resQuote / resBase

    let filterSwapTransactions (transactions: Transaction[]) =
        transactions |> Array.filter (fun transaction -> transaction.To = routerAddress && transaction.Input <> "0x")

    [<Event("Swap")>]
    type SwapEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "sender", 1, true )>]
            member val Sender = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "recipient", 2, true )>]
            member val Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("int256", "amount0", 3, false )>]
            member val Amount0 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int256", "amount1", 4, false )>]
            member val Amount1 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint160", "sqrtPriceX96", 5, false )>]
            member val SqrtPriceX96 = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint128", "liquidity", 6, false )>]
            member val Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int24", "tick", 7, false )>]
            member val Tick = Unchecked.defaultof<BigInteger> with get, set

    let getSingleInfoFromRouter (func:FunctionMessage) (event:SwapEventDTO) transactionInput =
        let amountIn = if event.Amount0 < 0I then event.Amount0 * (-1I) else event.Amount0
        let amountOut = if event.Amount1 < 0I then event.Amount1 * (-1I) else event.Amount1
        if func :? ExactOutputSingleFunction
        then let decodedInput = (new ExactOutputSingleFunction()).DecodeInput(transactionInput)
             (decodedInput.Params.TokenIn, decodedInput.Params.TokenOut, amountIn, amountOut)
        else if func :? ExactInputSingleFunction 
        then let decodedInput = (new ExactInputSingleFunction()).DecodeInput(transactionInput)
             (decodedInput.Params.TokenIn, decodedInput.Params.TokenOut, amountIn, amountOut)
        else ("", "", 0I, 0I)

    
    let getSimpleInfoFromRouter (func:FunctionMessage) (event:SwapEventDTO) transactionInput =
        let amountIn = if event.Amount0 < 0I then event.Amount0 * (-1I) else event.Amount0
        let amountOut = if event.Amount1 < 0I then event.Amount1 * (-1I) else event.Amount1
        if func :? ExactOutputFunction
        then let decodedInput = (new ExactOutputFunction()).DecodeInput(transactionInput)
             let tokenIn = "0x" + decodedInput.Params.Path.Slice(46, 66).ToHex()
             //let second = decoded.Params.Path.Slice(23, 43).ToHex()
             let tokenOut = "0x" + decodedInput.Params.Path.Slice(0, 20).ToHex()
             (tokenIn, tokenOut, amountIn, amountOut)
        else if func :? ExactInputFunction
        then let decodedInput = (new ExactInputFunction()).DecodeInput(transactionInput)
             let tokenIn = "0x" + decodedInput.Params.Path.Slice(0, 20).ToHex()
             //let second = decoded.Params.Path.Slice(23, 43).ToHex()
             let tokenOut = "0x" + decodedInput.Params.Path.Slice(46, 66).ToHex()
             (tokenIn, tokenOut, amountIn, amountOut)
        else ("", "", 0I, 0I)

    let multicallToCall (multicall:string) length index =
        "0x" + multicall.Substring(index, length)
    
    let getInfoFromRouter (transaction:Transaction) (transactionReceipt:TransactionReceipt) =
        let event = transactionReceipt.Logs.DecodeAllEvents<SwapEventDTO>().[0].Event
        if transaction.Input.StartsWith(multicallId)
        then if transaction.Input.Contains(exactInputId.Replace("0x", ""))
             then transaction.Input.IndexOf(exactInputId.Replace("0x", ""))
                  |> multicallToCall transaction.Input lengthForSimpleCall
                  |> getSimpleInfoFromRouter (new ExactInputFunction()) event
             else if transaction.Input.Contains(exactOutputId.Replace("0x", ""))
             then transaction.Input.IndexOf(exactOutputId.Replace("0x", ""))
                  |> multicallToCall transaction.Input lengthForSimpleCall
                  |> getSimpleInfoFromRouter (new ExactOutputFunction()) event
             else if transaction.Input.Contains(exactInputSingleId) 
             then transaction.Input.IndexOf(exactInputSingleId.Replace("0x", ""))
                  |> multicallToCall transaction.Input lengthForSingleCall
                  |> getSingleInfoFromRouter (new ExactInputSingleFunction()) event
             else if transaction.Input.Contains(exactOutputSingleId.Replace("0x", "")) 
             then transaction.Input.IndexOf(exactOutputSingleId.Replace("0x", ""))
                  |> multicallToCall transaction.Input lengthForSingleCall
                  |> getSingleInfoFromRouter (new ExactOutputSingleFunction()) event
             else ("", "", 0I, 0I)
        else if transaction.Input.Contains(exactInputId) 
             then getSimpleInfoFromRouter (new ExactInputFunction()) event transaction.Input
             else if transaction.Input.Contains(exactOutputId) 
             then getSimpleInfoFromRouter (new ExactOutputFunction()) event transaction.Input
             else if transaction.Input.Contains(exactInputSingleId) 
             then getSingleInfoFromRouter (new ExactInputSingleFunction()) event transaction.Input
             else if transaction.Input.Contains(exactOutputSingleId)
             then getSingleInfoFromRouter (new ExactOutputSingleFunction()) event transaction.Input
        else ("", "", 0I, 0I)

    let partlyBuildCandle (transactionsWithReceipts:Tuple<Transaction, TransactionReceipt>[]) token0Id token1Id (candle:Candle)
                          wasRequiredTransactionsInPeriodOfTime firstIterFlag =
        async{
            let mutable closePrice =  candle.close
            let mutable lowPrice = candle.low
            let mutable highPrice = candle.high
            let mutable openPrice = candle._open
            let mutable volume = candle.volume
            let mutable _wasRequiredTransactionsInPeriodOfTime = wasRequiredTransactionsInPeriodOfTime
            let mutable _firstIterFlag = firstIterFlag

            for transactionWithReceipt in transactionsWithReceipts do
                let (transaction, receipt) = transactionWithReceipt
                let (tokenInAddress, tokenOutAddress, 
                     amountIn, amountOut) = getInfoFromRouter transaction receipt
                if token0Id = tokenInAddress && token1Id = tokenOutAddress
                then _wasRequiredTransactionsInPeriodOfTime <- true
                     //printfn "\n\n\nTRANSACTION!!!\n%s\n\n" transaction.TransactionHash
                     let currentPrice = BigDecimal(amountIn, 0) / BigDecimal(amountOut, 0)
                     if _firstIterFlag then closePrice <- currentPrice
                                            _firstIterFlag <- false
                     if (currentPrice > highPrice) then highPrice <- currentPrice
                     if (currentPrice < lowPrice) then lowPrice <- currentPrice
                     openPrice <- currentPrice
                     volume <- volume + 1u
            return ({ 
                close = closePrice;
                low = lowPrice;
                high = highPrice;
                _open = openPrice;
                volume = volume;
            }, _wasRequiredTransactionsInPeriodOfTime, _firstIterFlag)
        }


    let buildCandleAsync (currentTime:DateTimeOffset) (resolutionTime:TimeSpan) resolutionTimeAgoUnix pairId (web3:Web3) = 
        
        let filterSuccessfulTranscations (transactionsWithReceipts:Tuple<Transaction, TransactionReceipt>[]) = 
            transactionsWithReceipts
            |> Array.filter (fun tr -> let (t, r) = tr 
                                       r.Status.Value <> 0I) 
        
        async{
            let pair = Requests.takePoolInfo(pairId).Value
            let token0Id = pair.token0Id
            let token1Id = pair.token1Id
            let mutable blockNumber = (currentTime.ToUnixTimeSeconds()
                                       |> BigInteger
                                       |> Dater.getBlockByDateAsync false web3//IfBlockAfterDate
                                       |> Async.RunSynchronously).number.Value
            let mutable block = HexBigInteger blockNumber
                                |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                                |> Async.AwaitTask
                                |> Async.RunSynchronously
            let mutable candle = { close = BigDecimal(0I, 0);
                                   low = BigDecimal.Parse maxUInt256StringRepresentation;
                                   high = BigDecimal(0I, 0);
                                   _open = BigDecimal(0I, 0);
                                   volume = 0u;
                                 }
            let mutable wasRequiredTransactionsInPeriodOfTime = false
            let mutable firstIterFlag = true

            while block.Timestamp.Value > resolutionTimeAgoUnix  do
                let swapTransactions = filterSwapTransactions block.Transactions |> Array.rev
                let map (swapTransaction:Transaction) = 
                    async{
                        return! web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTransaction.TransactionHash)
                                |> Async.AwaitTask                 
                    }
                let! swapTransactionReceipts =
                    swapTransactions
                    |> Array.map map
                    |> Async.Parallel
                let swapTransactionsWithReceipts = Array.map2 (fun transaction receipt -> (transaction, receipt)) 
                                                              swapTransactions swapTransactionReceipts
                let successfulSwapTransactionsWithReceipts = filterSuccessfulTranscations swapTransactionsWithReceipts 
                let! (_candle, _wasRequiredTransactionsInPeriodOfTime, 
                      _firstIterFlag) = partlyBuildCandle successfulSwapTransactionsWithReceipts token0Id token1Id candle
                                                          wasRequiredTransactionsInPeriodOfTime firstIterFlag
                candle <- _candle
                wasRequiredTransactionsInPeriodOfTime <- _wasRequiredTransactionsInPeriodOfTime
                firstIterFlag <- _firstIterFlag
                printfn "blockNumber = %A" blockNumber
                blockNumber <- blockNumber - 1I
                block <- HexBigInteger blockNumber
                         |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                         |> Async.AwaitTask
                         |> Async.RunSynchronously
            let dbCandle = if wasRequiredTransactionsInPeriodOfTime
                           then Some {
                               datetime = currentTime.DateTime;
                               resolutionSeconds = (int)resolutionTime.TotalSeconds;
                               uniswapPairId = pairId;
                               _open = candle._open.ToString();
                               high = candle.high.ToString();
                               low = candle.low.ToString();
                               close = candle.close.ToString();
                               volume = candle.volume;
                           }
                            else None
            return dbCandle
        }

    let sendCallbackAndWriteToDB candle (currentTime:DateTimeOffset) (resolutionTimeAgo:DateTimeOffset) callback = 
        match candle with
        | Some candle -> callback ($"uniswapPairId:{candle.uniswapPairId}\nresolutionSeconds:{candle.resolutionSeconds}\n"+
                                             $"datetime:{candle.datetime}\n_open:{candle._open}\nlow:{candle.low}\nhigh:{candle.high}\n"+
                                             $"close:{candle.close}\nvolume:{candle.volume}")
                         (DB.addCandle >> Async.RunSynchronously) candle
        | None -> callback $"No swaps\nfrom:{resolutionTimeAgo.DateTime}\nto:{currentTime.DateTime}"

    let buildCandleSendCallbackAndWriteToDBAsync (resolutionTime:TimeSpan) pairId callback (web3:Web3) (currentTime:DateTimeOffset) = 
        async{
            let resolutionTimeAgo = currentTime.Subtract(resolutionTime)
            let resolutionTimeAgoUnix = resolutionTimeAgo.ToUnixTimeSeconds() |> BigInteger
            let! dbCandle = buildCandleAsync currentTime resolutionTime resolutionTimeAgoUnix pairId web3
            sendCallbackAndWriteToDB dbCandle currentTime resolutionTimeAgo callback
        }
        

    let getCandle(pairId: string, callback, resolutionTime:TimeSpan, web3:Web3) =
        let timer = new Timer(resolutionTime.TotalMilliseconds)
        let candlesHandler = new ElapsedEventHandler(fun _ _ ->
                                                     DateTime.Now.ToUniversalTime()
                                                     |> DateTimeOffset
                                                     |> buildCandleSendCallbackAndWriteToDBAsync resolutionTime 
                                                                                                 pairId callback web3
                                                     |> Async.RunSynchronously
                                                     )
        timer.Elapsed.AddHandler(candlesHandler)
        timer.Start()
        while true do
        ()

    let firstBlockTimestamp = DateTime(2015, 7, 30, 3, 26, 28).ToUniversalTime()
        
    let getCandles (pairId, callback, resolutionTime:TimeSpan, web3:Web3) =
        let mutable currentTime = DateTime.Now.ToUniversalTime()
        while currentTime >= firstBlockTimestamp do
            currentTime
            |> DateTimeOffset
            |> buildCandleSendCallbackAndWriteToDBAsync resolutionTime pairId callback web3
            |> Async.RunSynchronously
            currentTime <- currentTime.Subtract(resolutionTime)

open Logic

[<EntryPoint>]
let main args =
    let pairId = "0x8ad599c3a0ff1de082011efddc58f1908eb6e6d8"
    let resolutionTime = new TimeSpan(0, 0, 30)
    let web3 = new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e")
    (pairId, (fun c -> printfn "%A" c), resolutionTime, web3) |> Logic.getCandle
    //(pairId, (fun c -> printfn "%A" c), resolutionTime, web3) |> Logic.getCandles
    0