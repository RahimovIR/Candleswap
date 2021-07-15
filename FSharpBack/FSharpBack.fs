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
open Contracts.UniswapV3Router.ContractDefinition
open Contracts.UniswapV2Router.ContractDefinition
open Contracts.UniswapV1Exchange.ContractDefinition
open Nethereum.Util
open System.Linq



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
              HttpHeaders = [||]
              OperationName = Some "q" }

        GraphQLClient.sendRequest connection request

    type Swap =
        { id: string
          amount0In: float
          amount0Out: float
          amount1In: float
          amount1Out: float
          timestamp: int64 }

    type PairInfo =
        { reserve0: BigInteger
          reserve1: BigInteger
          price0: float
          price1: float
          token0Id: string
          token1Id: string }

    type PoolInfo = { token0Id: string; token1Id: string }

    let mapSwaps (token: JToken Option) =
        let mapper (token: JProperty) =
            token.Value.["swaps"]
            |> Seq.map
                (fun x ->
                    { id = (string x.["id"])
                      amount0In = (float x.["amount0In"])
                      amount0Out = (float x.["amount0Out"])
                      amount1In = (float x.["amount1In"])
                      amount1Out = (float x.["amount1Out"])
                      timestamp = (int64 x.["timestamp"]) })

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> List.ofSeq
            |> Some
        | None -> None

    let mapPairInfo (token: JToken Option) =
        let mapper (token: JProperty) =
            let info = token.Value.["pair"]

            { reserve0 = (info.Value<decimal>("reserve0") |> BigInteger)
              reserve1 = (info.Value<decimal>("reserve1") |> BigInteger)
              price0 = (float info.["token0Price"])
              price1 = (float info.["token1Price"])
              token0Id = info.["token0"].["id"].ToString()
              token1Id = info.["token1"].["id"].ToString() }

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> Some
        | None -> None

    let mapPoolInfo (token: JToken Option) =
        let mapper (token: JProperty) =
            let info = token.Value.["pool"]

            { token0Id = info.["token0"].["id"].ToString()
              token1Id = info.["token1"].["id"].ToString() }

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> Some
        | None -> None

    let deserialize (data: string) =
        if String.IsNullOrWhiteSpace(data) then
            None
        else
            data |> JToken.Parse |> Some

    let allPr x = printfn "%A" x

    let uniswapV2 =
        "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"

    let uniswapV3 =
        "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v3"

    let takeSwaps idPair =
        idPair
        |> swapsQuery
        |> requestMaker uniswapV2
        |> deserialize
        |> mapSwaps

    let takePairInfo idPair =
        idPair
        |> pairInfoQuery
        |> requestMaker uniswapV2
        |> deserialize
        |> mapPairInfo

    let takePoolInfo idPair =
        idPair
        |> poolInfoQuery
        |> requestMaker uniswapV3
        |> deserialize
        |> mapPoolInfo

type Candle =
    { _open: BigDecimal
      high: BigDecimal
      low: BigDecimal
      close: BigDecimal
      volume: uint }

[<CLIMutable>]
type DBCandle =
    { datetime: DateTime
      resolutionSeconds: int
      uniswapPairId: string
      _open: string
      high: string
      low: string
      close: string
      volume: uint }

module DB =

    let private databaseFilename =
        __SOURCE_DIRECTORY__ + @"\Database\candles.db"

    let private connectionString =
        sprintf "Data Source=%s;Version=3;" databaseFilename

    let private connection = new SQLiteConnection(connectionString)
    do connection.Open()

    let private fetchCandlesSql = @"select datetime, resolutionSeconds, uniswapPairId, open as _open, high, low, close, volume from candles
        where uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds"

    let private insertCandleSql =
        "insert into candles(datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume) "
        + "values (@datetime, @resolutionSeconds, @uniswapPairId, @_open, @high, @low, @close, @volume)"

    let private updateCandleSql =
        "update candles set open = @_open, high = @high, low = @low, close = @close, volume = @volume) "
        + "values uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds and datetime = @datetime)"

    let private getCandleByDatetimeSql =
        "select datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume"
        + "from candles"
        + "where datetime = @datetime"

    let inline (=>) k v = k, box v

    let private dbQuery<'T> (connection: SQLiteConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
        match parameters with
        | Some (p) -> connection.QueryAsync<'T>(sql, p)
        | None -> connection.QueryAsync<'T>(sql)

    let private dbExecute (connection: SQLiteConnection) (sql: string) (data: _) = connection.ExecuteAsync(sql, data)

    let fetchCandles (uniswapPairId: string) (resolutionSeconds: int) =
        async {
            let! candles =
                Async.AwaitTask
                <| dbQuery<DBCandle>
                    connection
                    fetchCandlesSql
                    (Some(
                        dict [ "uniswapPairId" => uniswapPairId
                               "resolutionSeconds" => resolutionSeconds ]
                    ))

            return candles
        }


    let fetchCandlesTask (uniswapPairId: string) (resolutionSeconds: int) =
        Async.StartAsTask
        <| fetchCandles uniswapPairId resolutionSeconds

    let addCandle candle =
        async {
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection insertCandleSql candle

            printfn "records added: %i" rowsChanged
        }

    let updateCandle candle =
        async {
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection updateCandleSql candle

            printfn "records added: %i" rowsChanged
        }

    let getCandleByDatetime datetime =
        async {
            let queryParams = Some <| dict [ "datetime" => datetime ]

            let! candle =
                Async.AwaitTask
                <| dbQuery connection getCandleByDatetimeSql queryParams

            return candle
        }

module Dater =
    type BlockNumberTimestamp =
        { number: HexBigInteger
          timestamp: HexBigInteger }

    let getBlockNumberAndTimestampAsync
        (savedBlocks: Dictionary<HexBigInteger, HexBigInteger>)
        (web3: Web3)
        (blockNumber: HexBigInteger)
        =
        async {
            let timestamp = ref (HexBigInteger "0")

            if savedBlocks.TryGetValue(blockNumber, timestamp) then
                return
                    { number = blockNumber
                      timestamp = timestamp.Value }
            else
                let! block =
                    Async.AwaitTask
                    <| web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber)

                savedBlocks.Add(blockNumber, block.Timestamp)

                return
                    { number = blockNumber
                      timestamp = block.Timestamp }
        }

    let isBestBlock date predictedBlockTime previousBlockTime nextBlockTime ifBlockAfterDate =
        match ifBlockAfterDate with
        | true ->
            if predictedBlockTime < date then
                false
            else if predictedBlockTime >= date
                    && previousBlockTime < date then
                true
            else
                false
        | false ->
            if predictedBlockTime > date then
                false
            else if predictedBlockTime <= date && nextBlockTime > date then
                true
            else
                false

    let getNextPredictedBlockNumber currentPredictedBlockNumber skip =
        let nextPredictedBlockNumber = currentPredictedBlockNumber + skip

        if nextPredictedBlockNumber <= 1I then
            1I
        else
            nextPredictedBlockNumber

    let rec findBestBlock date predictedBlock ifBlockAfterDate blockTime savedBlocks checkedBlocks web3 =
        async {
            let! previousPredictedBlock =
                predictedBlock.number.Value - 1I
                |> HexBigInteger
                |> getBlockNumberAndTimestampAsync savedBlocks web3

            let! nextPredictedBlock =
                predictedBlock.number.Value + 1I
                |> HexBigInteger
                |> getBlockNumberAndTimestampAsync savedBlocks web3

            if isBestBlock
                date
                predictedBlock.timestamp.Value
                previousPredictedBlock.timestamp.Value
                nextPredictedBlock.timestamp.Value
                ifBlockAfterDate then
                return predictedBlock
            else
                let difference = date - predictedBlock.timestamp.Value

                let mutable skip =
                    (float difference) / blockTime |> Math.Ceiling

                if skip = 0.0 then
                    skip <- if difference < 0I then -1.0 else 1.0

                let! nextPredictedBlock =
                    getNextPredictedBlockNumber predictedBlock.number.Value (BigInteger skip)
                    |> HexBigInteger
                    |> getBlockNumberAndTimestampAsync savedBlocks web3

                let newBlockTime =
                    (predictedBlock.timestamp.Value
                     - nextPredictedBlock.timestamp.Value)
                    / (predictedBlock.number.Value
                       - nextPredictedBlock.number.Value)
                    |> float
                    |> Math.Abs

                return!
                    findBestBlock date nextPredictedBlock ifBlockAfterDate newBlockTime savedBlocks checkedBlocks web3
        }

    let getBlockByDateAsync ifBlockAfterDate (web3: Web3) date =
        async {
            let savedBlocks =
                new Dictionary<HexBigInteger, HexBigInteger>()

            let checkedBlocks = new List<BigInteger>()

            let! latestBlockNumber =
                web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()
                |> Async.AwaitTask

            let! latestBlock = getBlockNumberAndTimestampAsync savedBlocks web3 latestBlockNumber
            let firstBlockNumber = HexBigInteger 1I
            let! firstBlock = getBlockNumberAndTimestampAsync savedBlocks web3 firstBlockNumber

            let blockTime =
                (float (
                    latestBlock.timestamp.Value
                    - firstBlock.timestamp.Value
                ))
                / (float (latestBlock.number.Value - 1I))

            if date <= firstBlock.timestamp.Value then
                return firstBlock
            else if date >= latestBlock.timestamp.Value then
                return latestBlock
            else
                let! predictedBlock =
                    (float (date - firstBlock.timestamp.Value))
                    / blockTime
                    |> Math.Ceiling
                    |> BigInteger
                    |> HexBigInteger
                    |> getBlockNumberAndTimestampAsync savedBlocks web3

                return! findBestBlock date predictedBlock ifBlockAfterDate blockTime savedBlocks checkedBlocks web3
        }

    let convertToUnixTimestamp (date: DateTime) =
        let origin =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)

        let diff = date.ToUniversalTime() - origin
        Math.Floor(diff.TotalSeconds)


module Logic =

    let maxUInt256StringRepresentation =
        "115792089237316195423570985008687907853269984665640564039457584007913129639935"

    module SwapRouterV3 =
        let routerAddress =
            "0xe592427a0aece92de3edee1f18e0157c05861564"

        let exactInputSingleId = "0x414bf389"
        let exactOutputSingleId = "0xdb3e2198"
        let exactInputId = "0xc04b8d59"
        let exactOutputId = "0xf28c0498"
        let multicallId = "0xac9650d8"
        let lengthForSimpleCall = 648
        let lengthForSingleCall = 520

        [<Event("Swap")>]
        type SwapEventDTO() =
            inherit EventDTO()

            [<Parameter("address", "sender", 1, true)>]
            member val Sender = Unchecked.defaultof<string> with get, set

            [<Parameter("address", "recipient", 2, true)>]
            member val Recipient = Unchecked.defaultof<string> with get, set

            [<Parameter("int256", "amount0", 3, false)>]
            member val Amount0 = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("int256", "amount1", 4, false)>]
            member val Amount1 = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint160", "sqrtPriceX96", 5, false)>]
            member val SqrtPriceX96 = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint128", "liquidity", 6, false)>]
            member val Liquidity = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("int24", "tick", 7, false)>]
            member val Tick = Unchecked.defaultof<BigInteger> with get, set


        let getSingleInfoFromRouter (func: FunctionMessage) (event: SwapEventDTO) transactionInput =
            let amountIn =
                if event.Amount0 < 0I then
                    event.Amount0 * (-1I)
                else
                    event.Amount0

            let amountOut =
                if event.Amount1 < 0I then
                    event.Amount1 * (-1I)
                else
                    event.Amount1

            if func :? ExactOutputSingleFunction then
                let decodedInput =
                    (new ExactOutputSingleFunction())
                        .DecodeInput(transactionInput)

                (decodedInput.Params.TokenIn, decodedInput.Params.TokenOut, amountIn, amountOut)
            else if func :? ExactInputSingleFunction then
                let decodedInput =
                    (new ExactInputSingleFunction())
                        .DecodeInput(transactionInput)

                (decodedInput.Params.TokenIn, decodedInput.Params.TokenOut, amountIn, amountOut)
            else
                ("", "", 0I, 0I)


        let getSimpleInfoFromRouter (func: FunctionMessage) (event: SwapEventDTO) transactionInput =
            let getAmount amount = 
                if amount < 0I then amount * (-1I) else amount
            
            let amountIn = getAmount event.Amount0
            let amountOut = getAmount event.Amount1

            if func :? ExactOutputFunction then
                let decodedInput =
                    (new ExactOutputFunction())
                        .DecodeInput(transactionInput)

                let tokenIn =
                    "0x"
                    + decodedInput.Params.Path.Slice(46, 66).ToHex()
                //let second = decoded.Params.Path.Slice(23, 43).ToHex()
                let tokenOut =
                    "0x"
                    + decodedInput.Params.Path.Slice(0, 20).ToHex()

                (tokenIn, tokenOut, amountIn, amountOut)
            else if func :? ExactInputFunction then
                let decodedInput =
                    (new ExactInputFunction())
                        .DecodeInput(transactionInput)

                let tokenIn =
                    "0x"
                    + decodedInput.Params.Path.Slice(0, 20).ToHex()
                //let second = decoded.Params.Path.Slice(23, 43).ToHex()
                let tokenOut =
                    "0x"
                    + decodedInput.Params.Path.Slice(46, 66).ToHex()

                (tokenIn, tokenOut, amountIn, amountOut)
            else
                ("", "", 0I, 0I)

        let multicallToCall (multicall: string) length index =
            "0x" + multicall.Substring(index, length)

        let getInfoFromRouter (transaction: Transaction) (transactionReceipt: TransactionReceipt) =
            let swapEvents = transactionReceipt.Logs.DecodeAllEvents<SwapEventDTO>()
            if transaction.Input.StartsWith(multicallId) then
                if transaction.Input.Contains(exactInputId.Replace("0x", "")) then
                    transaction.Input.IndexOf(exactInputId.Replace("0x", ""))
                    |> multicallToCall transaction.Input lengthForSimpleCall
                    |> getSimpleInfoFromRouter (new ExactInputFunction())swapEvents.[0].Event
                else if transaction.Input.Contains(exactOutputId.Replace("0x", "")) then
                    transaction.Input.IndexOf(exactOutputId.Replace("0x", ""))
                    |> multicallToCall transaction.Input lengthForSimpleCall
                    |> getSimpleInfoFromRouter (new ExactOutputFunction()) swapEvents.[0].Event
                else if transaction.Input.Contains(exactInputSingleId) then
                    transaction.Input.IndexOf(exactInputSingleId.Replace("0x", ""))
                    |> multicallToCall transaction.Input lengthForSingleCall
                    |> getSingleInfoFromRouter (new ExactInputSingleFunction()) swapEvents.[0].Event
                else if transaction.Input.Contains(exactOutputSingleId.Replace("0x", "")) then
                    transaction.Input.IndexOf(exactOutputSingleId.Replace("0x", ""))
                    |> multicallToCall transaction.Input lengthForSingleCall
                    |> getSingleInfoFromRouter (new ExactOutputSingleFunction()) swapEvents.[0].Event
                else
                    ("", "", 0I, 0I)
            else if transaction.Input.Contains(exactInputId) then
                getSimpleInfoFromRouter (new ExactInputFunction()) swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(exactOutputId) then
                getSimpleInfoFromRouter (new ExactOutputFunction()) swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(exactInputSingleId) then
                getSingleInfoFromRouter (new ExactInputSingleFunction()) swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(exactOutputSingleId) then
                getSingleInfoFromRouter (new ExactOutputSingleFunction()) swapEvents.[0].Event transaction.Input
            else
                ("", "", 0I, 0I)

    module SwapRouterV2 =
        let router01Address = "0xf164fC0Ec4E93095b804a4795bBe1e041497b92a"
        let router02Address = "0x7a250d5630b4cf539739df2c5dacb4c659f2488d"

        let swapExactTokensForTokensId = "0x38ed1739"
        let swapTokensForExactTokensId = "0x8803dbee"
        let swapExactETHForTokensId = "0x7ff36ab5"
        let swapTokensForExactETHId = "0x4a25d94a"
        let swapExactTokensForETHId = "0x18cbafe5"
        let swapETHForExactTokensId = "0xfb3bdb41"
        let swapExactTokensForTokensSupportingFeeOnTransferTokensId = "0x5c11d795"
        let swapExactETHForTokensSupportingFeeOnTransferTokensId = "0xb6f9de95"
        let swapExactTokensForETHSupportingFeeOnTransferTokensId = "0x791ac947"

        [<Event("Swap")>]
        type SwapEventDTO() =
            inherit EventDTO()

            [<Parameter("address", "sender", 1, true)>]
            member val Sender = Unchecked.defaultof<string> with get, set

            [<Parameter("uint", "amount0In", 2, false)>]
            member val Amount0In = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint", "amount1In", 3, false)>]
            member val Amount1In = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint", "amount0Out", 4, false)>]
            member val Amount0Out = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint", "amount1Out", 5, false)>]
            member val Amount1Out = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("address", "to", 6, true)>]
            member val To = Unchecked.defaultof<string> with get, set

        let getTokensAndAmountsFromRouter (func: FunctionMessage) (event: SwapEventDTO) transactionInput = 
            let getAmount amount0 amount1 =
                if amount0 = 0I then amount1 else amount0

            let amountIn = getAmount event.Amount0In event.Amount1In
            let amountOut = getAmount event.Amount0Out event.Amount1Out

            if func :? SwapExactTokensForTokensFunction
            then let decoded = (new SwapExactTokensForTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapTokensForExactTokensFunction
            then let decoded = (new SwapTokensForExactTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapExactETHForTokensFunction
            then let decoded = (new SwapExactETHForTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapTokensForExactETHFunction
            then let decoded = (new SwapTokensForExactETHFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapExactTokensForETHFunction
            then let decoded = (new SwapExactTokensForETHFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapETHForExactTokensFunction
            then let decoded = (new SwapETHForExactTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
            then let decoded = (new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
            then let decoded = (new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else if func :? SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
            then let decoded = (new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
                 (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
            else ("", "", 0I, 0I)

        let getInfoFromRouter (transaction: Transaction) (transactionReceipt: TransactionReceipt) =
            let swapEvents = transactionReceipt.Logs.DecodeAllEvents<SwapEventDTO>()
            if transaction.Input.Contains(swapExactTokensForTokensId) 
            then getTokensAndAmountsFromRouter (new SwapExactTokensForTokensFunction()) swapEvents.[0].Event 
                                                                                        transaction.Input
            else if transaction.Input.Contains(swapTokensForExactTokensId) 
            then getTokensAndAmountsFromRouter (new SwapTokensForExactTokensFunction()) swapEvents.[0].Event  
                                                                                        transaction.Input
            else if transaction.Input.Contains(swapExactETHForTokensId)
            then getTokensAndAmountsFromRouter (new SwapExactETHForTokensFunction())swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(swapTokensForExactETHId) 
            then getTokensAndAmountsFromRouter (new SwapTokensForExactETHFunction()) swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(swapExactTokensForETHId) 
            then getTokensAndAmountsFromRouter (new SwapExactTokensForETHFunction()) swapEvents.[0].Event  transaction.Input
            else if transaction.Input.Contains(swapETHForExactTokensId) 
            then getTokensAndAmountsFromRouter (new SwapETHForExactTokensFunction()) swapEvents.[0].Event  transaction.Input
            else if transaction.Input.Contains(swapExactTokensForTokensSupportingFeeOnTransferTokensId) 
            then getTokensAndAmountsFromRouter (new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction()) 
                                               swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(swapExactETHForTokensSupportingFeeOnTransferTokensId) 
            then getTokensAndAmountsFromRouter (new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction()) 
                                               swapEvents.[0].Event transaction.Input
            else if transaction.Input.Contains(swapExactTokensForETHSupportingFeeOnTransferTokensId) 
            then getTokensAndAmountsFromRouter (new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction())
                                               swapEvents.[0].Event transaction.Input
            else ("", "", 0I, 0I)

    module ExchangeV1 =
        let exchangeAddress = "0x09cabec1ead1c0ba254b09efb3ee13841712be14"
        let wethAddress = "0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2"

        let ethToTokenSwapInputId = "0xf39b5b9b"
        let ethToTokenTransferInputId = "0xad65d76d"
        let ethToTokenSwapOutputId = "0x6b1d4db7"
        let ethToTokenTransferOutputId = "0x0b573638"

        let tokenToEthSwapInputId = "0x95e3c50b"
        let tokenToEthTransferInputId = "0x7237e031"
        let tokenToEthSwapOutputId = "0x013efd8b"
        let tokenToEthTransferOutputId = "0xd4e4841d"

        let tokenToTokenSwapInputId = "0xddf7e1a7"
        let tokenToTokenTransferInputId = "0xf552d91b"
        let tokenToTokenSwapOutputId = "0xb040d545"
        let tokenToTokenTransferOutputId = "0xf3c0efe9"

        let tokenToExchangeSwapInputId = "0xb1cb43bf"
        let tokenToExchangeTransferInputId = "0xec384a3e"
        let tokenToExchangeSwapOutputId = "0xea650c7d"
        let tokenToExchangeTransferOutputId = "0x981a1327"


        [<Event("TokenPurchase")>]
        type TokenPurchaseEventDTO() =
            inherit EventDTO()

            [<Parameter("address", "buyer", 1, true)>]
            member val Buyer = Unchecked.defaultof<string> with get, set

            [<Parameter("uint256", "eth_sold", 2, true)>]
            member val EthSold = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint256", "tokens_bought", 3, true)>]
            member val TokensBought = Unchecked.defaultof<BigInteger> with get, set


        [<Event("EthPurchase")>]
        type EthPurchaseEventDTO() =
            inherit EventDTO()

            [<Parameter("address", "buyer", 1, true)>]
            member val Buyer = Unchecked.defaultof<string> with get, set

            [<Parameter("uint256", "tokens_sold", 2, true)>]
            member val TokensSold = Unchecked.defaultof<BigInteger> with get, set

            [<Parameter("uint256", "eth_bought", 3, true)>]
            member val EthBought = Unchecked.defaultof<BigInteger> with get, set


        let getInfoFromEthToToken (tokenPurchaseEvent: TokenPurchaseEventDTO) = 
            let tokenIn = wethAddress
            let tokenOut = tokenPurchaseEvent.Buyer 
            let amountIn = tokenPurchaseEvent.EthSold
            let amountOut = tokenPurchaseEvent.TokensBought
            (tokenIn, tokenOut, amountIn, amountOut)

        let getInfoFromTokenToEth (ethPurchaseEvent: EthPurchaseEventDTO) = 
            let tokenIn = ethPurchaseEvent.Buyer
            let tokenOut = wethAddress
            let amountIn = ethPurchaseEvent.TokensSold
            let amountOut = ethPurchaseEvent.EthBought
            (tokenIn, tokenOut, amountIn, amountOut)

        let getInfoFromTokenToToken (firstTransferEvent:TransferEventDTO) (secondTransferEvent:TransferEventDTO)=
            let tokenIn = firstTransferEvent.From
            let tokenOut = secondTransferEvent.From
            let amountIn = firstTransferEvent.Value
            let amountOut = secondTransferEvent.Value
            (tokenIn, tokenOut, amountIn, amountOut)
        

        let getInfoFromExchange (transaction: Transaction) (transactionReceipt: TransactionReceipt) =
            let transferEvents = transactionReceipt.Logs.DecodeAllEvents<TransferEventDTO>()
            let tokenPurchaseEvents = transactionReceipt.Logs.DecodeAllEvents<TokenPurchaseEventDTO>()
            let ethPurchaseEvents = transactionReceipt.Logs.DecodeAllEvents<EthPurchaseEventDTO>()
            if transaction.Input.Contains(ethToTokenSwapInputId) || transaction.Input.Contains(ethToTokenTransferInputId)
               || transaction.Input.Contains(ethToTokenSwapOutputId) || transaction.Input.Contains(ethToTokenTransferOutputId)
            then getInfoFromEthToToken tokenPurchaseEvents.[0].Event
            else if transaction.Input.Contains(tokenToEthSwapInputId) || transaction.Input.Contains(tokenToEthTransferInputId)
                    || transaction.Input.Contains(tokenToEthSwapOutputId) || transaction.Input.Contains(tokenToEthTransferOutputId)
            then getInfoFromTokenToEth ethPurchaseEvents.[0].Event
            else if transaction.Input.Contains(tokenToTokenSwapInputId) || transaction.Input.Contains(tokenToTokenTransferInputId)
                    || transaction.Input.Contains(tokenToTokenSwapOutputId) || transaction.Input.Contains(tokenToTokenTransferOutputId)
                    || transaction.Input.Contains(tokenToExchangeSwapInputId) || transaction.Input.Contains(tokenToExchangeTransferInputId)
                    || transaction.Input.Contains(tokenToExchangeSwapOutputId) || transaction.Input.Contains(tokenToExchangeTransferOutputId)
            then getInfoFromTokenToToken transferEvents.[0].Event transferEvents.[1].Event
            else ("", "", 0I, 0I)

    let partlyBuildCandle
        (transactionsWithReceipts: Tuple<Transaction, TransactionReceipt> [])
        token0Id
        token1Id
        (candle: Candle)
        wasRequiredTransactionsInPeriodOfTime
        firstIterFlag
        =
        async {
            let mutable closePrice = candle.close
            let mutable lowPrice = candle.low
            let mutable highPrice = candle.high
            let mutable openPrice = candle._open
            let mutable volume = candle.volume
            let mutable _wasRequiredTransactionsInPeriodOfTime = wasRequiredTransactionsInPeriodOfTime
            let mutable _firstIterFlag = firstIterFlag

            for transactionWithReceipt in transactionsWithReceipts do
                let (transaction, receipt) = transactionWithReceipt
                let (tokenInAddress, tokenOutAddress, amountIn, amountOut) = if transaction.To = SwapRouterV3.routerAddress 
                                                                             then SwapRouterV3.getInfoFromRouter transaction receipt
                                                                             else if transaction.To = SwapRouterV2.router02Address ||
                                                                                     transaction.To = SwapRouterV2.router01Address
                                                                             then SwapRouterV2.getInfoFromRouter transaction receipt
                                                                             else if transaction.To = ExchangeV1.exchangeAddress
                                                                             then ExchangeV1.getInfoFromExchange transaction receipt
                                                                             else ("", "", 0I, 0I)

                if token0Id = tokenInAddress
                   && token1Id = tokenOutAddress then
                    _wasRequiredTransactionsInPeriodOfTime <- true
                    //printfn "\n\n\nTRANSACTION!!!\n%s\n\n" transaction.TransactionHash
                    let currentPrice =
                        BigDecimal(amountIn, 0) / BigDecimal(amountOut, 0)

                    if _firstIterFlag then
                        closePrice <- currentPrice
                        _firstIterFlag <- false

                    if (currentPrice > highPrice) then
                        highPrice <- currentPrice

                    if (currentPrice < lowPrice) then
                        lowPrice <- currentPrice

                    openPrice <- currentPrice
                    volume <- volume + 1u

            return
                ({ close = closePrice
                   low = lowPrice
                   high = highPrice
                   _open = openPrice
                   volume = volume },
                 _wasRequiredTransactionsInPeriodOfTime,
                 _firstIterFlag)
        }

    let buildCandleAsync
        (currentTime: DateTimeOffset)
        (resolutionTime: TimeSpan)
        resolutionTimeAgoUnix
        pairId
        (web3: Web3)
        =

        let filterSwapTransactions (transactions: Transaction []) =
            transactions
            |> Array.filter
                (fun transaction ->
                    (transaction.To = SwapRouterV3.routerAddress 
                    || transaction.To = SwapRouterV2.router01Address
                    || transaction.To = SwapRouterV2.router02Address
                    || transaction.To = ExchangeV1.exchangeAddress)
                    && transaction.Input <> "0x")

        let filterSuccessfulTranscations (transactionsWithReceipts: Tuple<Transaction, TransactionReceipt> []) =
            transactionsWithReceipts
            |> Array.filter
                (fun tr ->
                    let (_, r) = tr
                    r.Status.Value <> 0I)

        async {
            let pair = Requests.takePoolInfo(pairId).Value
            let token0Id = pair.token0Id
            let token1Id = pair.token1Id
            //try
            let! initializableBlockNumber = currentTime.ToUnixTimeSeconds()
                                            |> BigInteger
                                            |> Dater.getBlockByDateAsync false web3
            let mutable blockNumber = initializableBlockNumber.number.Value

            let! initializableBlock = HexBigInteger blockNumber
                                      |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                                      |> Async.AwaitTask
            let mutable block = initializableBlock

            let mutable candle =
                { close = BigDecimal(0I, 0)
                  low = BigDecimal.Parse maxUInt256StringRepresentation
                  high = BigDecimal(0I, 0)
                  _open = BigDecimal(0I, 0)
                  volume = 0u }

            let mutable wasRequiredTransactionsInPeriodOfTime = false
            let mutable firstIterFlag = true

            while block.Timestamp.Value > resolutionTimeAgoUnix do
                let swapTransactions =
                    filterSwapTransactions block.Transactions
                    |> Array.rev

                let map (swapTransaction: Transaction) =
                    async {
                        return!
                            web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTransaction.TransactionHash)
                            |> Async.AwaitTask
                    }

                let! swapTransactionReceipts =
                    swapTransactions
                    |> Array.map map
                    |> Async.Parallel

                let swapTransactionsWithReceipts =
                    Array.map2 (fun transaction receipt -> (transaction, receipt)) swapTransactions swapTransactionReceipts

                let successfulSwapTransactionsWithReceipts =
                    filterSuccessfulTranscations swapTransactionsWithReceipts

                let! (_candle, _wasRequiredTransactionsInPeriodOfTime, _firstIterFlag) =
                    partlyBuildCandle
                        successfulSwapTransactionsWithReceipts
                        token0Id
                        token1Id
                        candle
                        wasRequiredTransactionsInPeriodOfTime
                        firstIterFlag

                candle <- _candle
                wasRequiredTransactionsInPeriodOfTime <- _wasRequiredTransactionsInPeriodOfTime
                firstIterFlag <- _firstIterFlag
                printfn "blockNumber = %A" blockNumber
                blockNumber <- blockNumber - 1I

                let! helpfulBlock = HexBigInteger blockNumber
                                   |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                                   |> Async.AwaitTask
                block <- helpfulBlock

            let dbCandle =
                if wasRequiredTransactionsInPeriodOfTime then
                    Some
                        { datetime = currentTime.DateTime
                          resolutionSeconds = (int) resolutionTime.TotalSeconds
                          uniswapPairId = pairId
                          _open = candle._open.ToString()
                          high = candle.high.ToString()
                          low = candle.low.ToString()
                          close = candle.close.ToString()
                          volume = candle.volume }
                else
                    None

            return dbCandle
            (*with
            | _ -> printfn "\n\nERROR!!!\n\n currentTime = %s" <| currentTime.ToString()
                   return None*)
        }

    let sendCallbackAndWriteToDB candle (currentTime: DateTimeOffset) (resolutionTimeAgo: DateTimeOffset) callback =
        match candle with
        | Some candle ->
            callback (
                $"uniswapPairId:{candle.uniswapPairId}\nresolutionSeconds:{candle.resolutionSeconds}\n"
                + $"datetime:{candle.datetime}\n_open:{candle._open}\nlow:{candle.low}\nhigh:{candle.high}\n"
                + $"close:{candle.close}\nvolume:{candle.volume}"
            )

            DB.addCandle candle
        | None -> async{ callback $"No swaps\nfrom:{resolutionTimeAgo.DateTime}\nto:{currentTime.DateTime}"}

    let buildCandleSendCallbackAndWriteToDBAsync
        (resolutionTime: TimeSpan)
        pairId
        callback
        (web3: Web3)
        (currentTime: DateTimeOffset)
        =
        async {
            let resolutionTimeAgo = currentTime.Subtract(resolutionTime)

            let resolutionTimeAgoUnix =
                resolutionTimeAgo.ToUnixTimeSeconds()
                |> BigInteger

            let! dbCandle = buildCandleAsync currentTime resolutionTime resolutionTimeAgoUnix pairId web3
            return! sendCallbackAndWriteToDB dbCandle currentTime resolutionTimeAgo callback
        }

    let getCandle (pairId: string, callback, resolutionTime: TimeSpan, web3: Web3) =
        let timer =
            new Timer(resolutionTime.TotalMilliseconds)

        let candlesHandler =
            new ElapsedEventHandler(fun _ _ ->
                DateTime.Now.ToUniversalTime()
                |> DateTimeOffset
                |> buildCandleSendCallbackAndWriteToDBAsync resolutionTime pairId callback web3
                |> Async.RunSynchronously)

        timer.Elapsed.AddHandler(candlesHandler)
        timer.Start()

        while true do
            ()

    let firstUniswapExchangeTimestamp =
        DateTime(2018, 11, 2, 10, 33, 56).ToUniversalTime()

    let getCandles (pairId, callback, resolutionTime: TimeSpan, web3: Web3) =
        let mutable currentTime = DateTime.Now.ToUniversalTime()
        //let mutable currentTime = (new DateTime(2018, 11, 2, 10, 34, 56)).ToUniversalTime()

        while currentTime >= firstUniswapExchangeTimestamp do
            currentTime
            |> DateTimeOffset
            |> buildCandleSendCallbackAndWriteToDBAsync resolutionTime pairId callback web3
            |> Async.RunSynchronously
            currentTime <- currentTime.Subtract(resolutionTime)
        callback "Processing completed successfully"


open Logic

[<EntryPoint>]
let main args =
    let pairId =
        "0x8ad599c3a0ff1de082011efddc58f1908eb6e6d8"

    let resolutionTime = new TimeSpan(0, 5, 0)

    let web3 =
        new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e")
    
    //(pairId, (fun c -> printfn "%A" c), resolutionTime, web3) |> Logic.getCandle
    (pairId, (fun c -> printfn "%A" c), resolutionTime, web3) |> Logic.getCandles
    0
