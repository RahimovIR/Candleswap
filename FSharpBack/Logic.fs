namespace RedDuck.Candleswap.Candles

open System
open System.Timers
open System.Numerics
open Nethereum.Web3
open Nethereum.Hex.HexTypes
open Nethereum.RPC.Eth.DTOs
open Nethereum.Util
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic
open Domain.Types
open Domain
open Indexer.Dater

module Logic =
    let maxUInt256StringRepresentation =
        "115792089237316195423570985008687907853269984665640564039457584007913129639935"
 
    let calculateCandlesByTransactions connection (swapTransactions:DbTransaction seq) =
        async{
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
                swapTransactions 
                |> Seq.map (fun swapTransaction -> 
                                 let pair  = 
                                     {id = 0L; 
                                      token0Id = swapTransaction.token0Id; 
                                      token1Id = swapTransaction.token1Id;}
                                 ((HexBigInteger swapTransaction.amountIn).Value, 
                                  (HexBigInteger swapTransaction.amountOut).Value, pair))

            let pairsAmounts = amountsPairArray
                               |> Seq.groupBy (fun (_, _, pair) -> pair)

            
            do! pairsAmounts
                |> Seq.map (fun (pair, _) -> pair)
                |> Db.addPairsIfNotExistAsync connection            

            let pairsCandles = pairsAmounts
                               |> Seq.map (fun (pair, list) -> (pair, calculateCandle list))
            return pairsCandles
        }

    let buildCandleAsync
        connection
        (web3: IWeb3)
        startTime
        endTime
        =
        async {
            printfn "New pass"

            let! startBlockNumber = getBlockNumberByDateTimeOffsetAsync false web3 startTime
            let! endBlockNumber = getBlockNumberByDateTimeOffsetAsync false web3 endTime

            let! transactions = Db.fetchTransactionsBetweenBlocks connection startBlockNumber endBlockNumber
                                
            let! pairsWithCandles = calculateCandlesByTransactions connection transactions

            let! pairsWithDbCandles = 
                pairsWithCandles
                |> Seq.map (fun pairWithCandle -> 
                           async{
                               let (pair, candle) = pairWithCandle
                               let! pairFromDb = Db.fetchPairAsync connection pair.token0Id pair.token1Id
                               let dbCandle = { datetime = (DateTimeOffset startTime).ToUnixTimeSeconds()
                                                resolutionSeconds = (int)(startTime - endTime).TotalSeconds
                                                pairId = pairFromDb.Value.id 
                                                _open = candle._open.ToString()
                                                high = candle.high.ToString()
                                                low = candle.low.ToString()
                                                close = candle.close.ToString()
                                                volume = (int)candle.volume}
                               return struct (pair, dbCandle)
                           })
                |> Async.Parallel
            
            return pairsWithDbCandles
        }

    let sendCallbackAndWriteToDBAsync
        connection 
        (pairsWithDbCandles: struct (Pair*DbCandle) [])
        callback =
        async{
            for pairWithDbCandle in pairsWithDbCandles do
                let struct (_, dbCandle) = pairWithDbCandle
                callback((pairWithDbCandle))
                do! Db.addCandle connection dbCandle
        }

    let buildCandleSendCallbackAndWriteToDBAsync
        connection
        (resolutionTime: TimeSpan)
        callback
        (web3: IWeb3)
        (currentTime: DateTime)
        =
        async {
            let endTime = currentTime.Subtract(resolutionTime)

            let! pairsWithDbCandles = buildCandleAsync connection web3 currentTime endTime
            return! sendCallbackAndWriteToDBAsync connection pairsWithDbCandles callback
        }

    let getCandle connection callback (resolutionTime: TimeSpan) (web3: IWeb3) (cancelToken:CancellationToken) =
        async{
            try
                let mutable currentTime = DateTime.UtcNow
                while cancelToken.IsCancellationRequested <> true do
                    let timer = new System.Diagnostics.Stopwatch()
                    timer.Start()
                    do! buildCandleSendCallbackAndWriteToDBAsync connection resolutionTime callback web3 currentTime
                    currentTime <- currentTime + resolutionTime
                    //do! Task.Delay(resolutionTime - timer.Elapsed) |> Async.AwaitTask
                    do! Task.Delay(resolutionTime) |> Async.AwaitTask
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
                inner (a.Add rate, b) (a :: samples)
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
                else printfn "Processing completed successfully"
            with
            | ex -> ex.ToString() |> printfn "%s"
        }
