namespace RedDuck.Candleswap.Candles

open System
open System.Timers
open System.Numerics
open Nethereum.Web3
open Nethereum.Hex.HexTypes
open Nethereum.RPC.Eth.DTOs
open Nethereum.Util
open RedDuck.Candleswap.Candles
open RedDuck.Candleswap.Candles.Types
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic

module Logic =
    let maxUInt256StringRepresentation =
        "115792089237316195423570985008687907853269984665640564039457584007913129639935"
 
    let calculateCandlesByTransactions connection (transactionsWithReceipts: (Transaction * TransactionReceipt) list) =
        async{
            let tryGetTransactionInfo addresses infoFunc (transaction: Transaction) =
                if List.contains transaction.To addresses
                then Some infoFunc
                else None
        
            /// Returns Some function to obtain transaction information if transaction recipient 
            /// matches any known.
            let transactionData transaction receipt = 
                [tryGetTransactionInfo SwapRouterV2.addresses SwapRouterV2.getInfoFromRouter] 
                |> List.map (fun f -> f transaction)
                |> List.tryFind Option.isSome 
                |> Option.get
                |> Option.map (fun f -> f transaction receipt)

            /// Extended information about transactions to Uniswap contracts.
            let actualTransactionsData = 
                transactionsWithReceipts
                |> List.map (fun (t, r) -> transactionData t r)
                |> List.choose id

            let calculateCandle amounts =
                let currentPrice (amountIn, amountOut, _) = BigDecimal(amountIn, 0) / BigDecimal(amountOut, 0)//tokenOut per tokenIn
                //let currentPrice (_, _, amountIn, amountOut) = BigDecimal(amountOut, 0) / BigDecimal(amountIn, 0)//tokenIn per tokenOut
                let openPrice = (Seq.last >> currentPrice) amounts
                let closePrice = (Seq.head >> currentPrice) amounts

                let mutable initHigh = BigDecimal(0I, 0)
                let mutable initLow = BigDecimal.Parse maxUInt256StringRepresentation
                let mutable initVolume = 0u

                let high, low, volume = 
                    let folder (high, low, volume) price =
                        (if high > price then high else price),
                        (if low < price then low else price),
                        (volume + 1u)
                    amounts
                    |> Seq.map currentPrice
                    |> Seq.fold folder (initHigh, initLow, initVolume)

                { _open = openPrice
                  low = low
                  high = high
                  close = closePrice
                  volume = volume }

            let amountsPairArray =
                actualTransactionsData 
                |> List.map (fun (token0Id, token1Id, amountIn, amountOut) -> 
                                  let pair  = {id = 0L; token0Id = token0Id; token1Id = token1Id}
                                  (amountIn, amountOut, pair))

            let pairsAmounts = amountsPairArray
                               |> Seq.groupBy (fun (_, _, pair) -> pair)

            
            do! pairsAmounts
                |> Seq.map (fun (pair, _) -> pair)
                |> Db.AddPairsIfNotExistAsync connection            

            let pairsCandles = pairsAmounts
                               |> Seq.map (fun (pair, list) -> (pair, calculateCandle list))
            return pairsCandles
            (*let mutable pairsCandles = []
            for pairAmounts in pairsAmounts do
                let newPairCandle = (pairAmounts.Key, List.ofSeq pairAmounts.Value 
                                                      |> calculateCandle) 
                pairsCandles <- newPairCandle::pairsCandles
            return pairsCandles*)
        }

    let buildCandleAsync
        connection
        (currentTime: DateTimeOffset)
        (resolutionTime: TimeSpan)
        resolutionTimeAgoUnix
        (web3: IWeb3)
        =

        let filterSwapTransactions (transactions: Transaction list) =
            transactions
            |> List.filter
                (fun transaction ->
                    (List.tryFind(fun address -> address = transaction.To) SwapRouterV2.addresses) <> None
                    && transaction.Input <> "0x"
                    && SwapRouterV2.swapFunctionIds
                       |> List.exists (fun func -> 
                       transaction.Input.Contains(func)))

        let filterSuccessfulTranscations (transactionsWithReceipts: Tuple<Transaction, TransactionReceipt> list) =
            transactionsWithReceipts
            |> List.filter
                (fun tr ->
                    let (_, r) = tr
                    r.Status.Value <> 0I)

        async {
            printfn "New pass"

            let! initializableBlockNumber = currentTime.ToUnixTimeSeconds()
                                            |> BigInteger
                                            |> Dater.getBlockByDateAsync false web3
            let mutable blockNumber = initializableBlockNumber.number.Value

            let! initializableBlock = HexBigInteger blockNumber
                                      |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                                      |> Async.AwaitTask
            let mutable block = initializableBlock

            let mutable successfulSwapTransactionsInPeriod = []

            while block.Timestamp.Value > resolutionTimeAgoUnix do

                let swapTransactions =
                    block.Transactions
                    |> List.ofArray
                    |> filterSwapTransactions
                    |> List.rev

                let map (swapTransaction: Transaction) =
                    async {
                        return!
                            web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(swapTransaction.TransactionHash)
                            |> Async.AwaitTask
                    }

                let! swapTransactionReceiptsArray =
                    swapTransactions
                    |> List.map map
                    |> Async.Parallel

                let swapTransactionReceipts = List.ofArray swapTransactionReceiptsArray

                let swapTransactionsWithReceipts =
                    List.map2 (fun transaction receipt -> (transaction, receipt)) swapTransactions swapTransactionReceipts

                let successfulSwapTransactionsWithReceipts =
                    filterSuccessfulTranscations swapTransactionsWithReceipts

                successfulSwapTransactionsInPeriod <- successfulSwapTransactionsInPeriod@successfulSwapTransactionsWithReceipts

                printfn "blockNumber = %A" blockNumber
                blockNumber <- blockNumber - 1I

                let! helpfulBlock = HexBigInteger blockNumber
                                   |> web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync
                                   |> Async.AwaitTask
                block <- helpfulBlock

            let! pairsWithCandles = calculateCandlesByTransactions connection successfulSwapTransactionsInPeriod

            let mutable pairsWithDbCandles = []

            let currentTimeUnix = currentTime.ToUnixTimeSeconds()

            for pairWithCandle in pairsWithCandles do
                let (pair, candle) = pairWithCandle
                let! pairFromDb = Db.fetchPairAsync connection pair.token0Id pair.token1Id
                let dbCandle = { datetime = currentTimeUnix
                                 resolutionSeconds = (int)resolutionTime.TotalSeconds
                                 pairId = pairFromDb.Value.id 
                                 _open = candle._open.ToString()
                                 high = candle.high.ToString()
                                 low = candle.low.ToString()
                                 close = candle.close.ToString()
                                 volume = (int)candle.volume}
                pairsWithDbCandles <- (pair, dbCandle)::pairsWithDbCandles

            return pairsWithDbCandles
        }

    let sendCallbackAndWriteToDBAsync
        connection 
        (pairsWithDbCandles: (Pair*DbCandle) list)
        callback =
        async{
            for pairWithDbCandle in pairsWithDbCandles do
                let (pair, dbCandle) = pairWithDbCandle
                callback(
                  $"token0Id:{pair.token0Id};\ntoken1Id:{pair.token1Id};\nresolutionSeconds:{dbCandle.resolutionSeconds};\n"
                + $"datetime:{dbCandle.datetime};\n_open:{dbCandle._open};\nlow:{dbCandle.low};\nhigh:{dbCandle.high};\n"
                + $"close:{dbCandle.close};\nvolume:{dbCandle.volume};")
                do! Db.addCandle connection dbCandle
        }

    let buildCandleSendCallbackAndWriteToDBAsync
        connection
        (resolutionTime: TimeSpan)
        callback
        (web3: IWeb3)
        (currentTime: DateTimeOffset)
        =
        async {
            let resolutionTimeAgo = currentTime.Subtract(resolutionTime)

            let resolutionTimeAgoUnix =
                resolutionTimeAgo.ToUnixTimeSeconds()
                |> BigInteger

            let! pairsWithDbCandles = buildCandleAsync connection currentTime resolutionTime resolutionTimeAgoUnix web3
            return! sendCallbackAndWriteToDBAsync connection pairsWithDbCandles callback
        }

    let getCandle connection callback (resolutionTime: TimeSpan) (web3: IWeb3) (cancelToken:CancellationToken) =
        async{
            try
                let mutable currentTime = DateTime.Now.ToUniversalTime() |> DateTimeOffset
                while cancelToken.IsCancellationRequested <> true do
                    let timer = new System.Diagnostics.Stopwatch()
                    timer.Start()
                    do! buildCandleSendCallbackAndWriteToDBAsync connection resolutionTime callback web3 currentTime
                    currentTime <- currentTime + resolutionTime
                    do! Task.Delay(resolutionTime - timer.Elapsed) |> Async.AwaitTask
                printfn "Operation was canceled!"
            with
            | ex -> ex.ToString() |> printfn "%s"
        }

    let firstUniswapExchangeTimestamp =
        DateTime(2018, 11, 2, 10, 33, 56).ToUniversalTime()

    let pancakeLauchDateTimestamp = 
        DateTime(2020, 9, 20, 0, 0, 0).ToUniversalTime()

    let getTimeSamples (period: DateTime * DateTime) rate =
        let rec inner (a: DateTime, b) samples =
            if a >= b then 
                samples
            else
                inner (a.Add rate, b) (DateTimeOffset a :: samples)
        inner period []

    let getCandles connection callback (resolutionTime: TimeSpan) (web3: IWeb3) (cancelToken:CancellationToken) startFrom =
        async{
            try
                let b = startFrom
                let a = pancakeLauchDateTimestamp
                let timeSamples = getTimeSamples(a, b) resolutionTime

                timeSamples
                |> List.takeWhile (
                   fun t -> async{
                                try
                                    do! buildCandleSendCallbackAndWriteToDBAsync connection resolutionTime callback web3 t
                                with
                                | ex -> ex.ToString() |> printfn "%s"
                            } |> Async.RunSynchronously
                            cancelToken.IsCancellationRequested <> true
                   ) |> ignore
                if cancelToken.IsCancellationRequested = true
                then printfn "Operation was canceled!"
                else callback "Processing completed successfully"
            with
            | ex -> ex.ToString() |> printfn "%s"
        }
