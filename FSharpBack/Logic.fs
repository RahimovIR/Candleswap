﻿namespace RedDuck.Candleswap.Candles

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
        startBlockNumber
        endBlockNumber
        =

        let rec getBlockFromDbOrDelayWhileNotIndexedAsync connection blockNumber = 
            async{
                let! blocks = Db.fetchBlockByNumber connection blockNumber
                match Seq.tryLast blocks with 
                | Some block -> return block
                | None -> do! Task.Delay(1000) |> Async.AwaitTask
                          return! getBlockFromDbOrDelayWhileNotIndexedAsync connection blockNumber      
            }

        async {
            printfn "New pass"

            let! startBlock = getBlockFromDbOrDelayWhileNotIndexedAsync connection startBlockNumber
                              
            let! endBlock = getBlockFromDbOrDelayWhileNotIndexedAsync connection endBlockNumber

            let resolution = (HexBigInteger startBlock.timestamp).Value - (HexBigInteger endBlock.timestamp).Value 

            let! transactions = Db.fetchTransactionsBetweenBlocks connection startBlockNumber endBlockNumber
                                
            let! pairsWithCandles = calculateCandlesByTransactions connection transactions

            let! pairsWithDbCandles = 
                pairsWithCandles
                |> Seq.map (fun pairWithCandle -> 
                           async{
                               let (pair, candle) = pairWithCandle
                               let! pairFromDb = Db.fetchPairAsync connection pair.token0Id pair.token1Id
                               let dbCandle = { datetime = (int64)(HexBigInteger startBlock.timestamp).Value
                                                resolutionSeconds = (int)resolution
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
        callback
        startBlockNumber
        endBlockNumber
        =
        async {
            let! pairsWithDbCandles = buildCandleAsync connection startBlockNumber endBlockNumber
            return! sendCallbackAndWriteToDBAsync connection pairsWithDbCandles callback
        }

    let getCandle connection (web3:IWeb3) callback (resolutionTime: TimeSpan) (cancelToken:CancellationToken) =
        async{
            try
                while cancelToken.IsCancellationRequested <> true do
                    let timer = new System.Diagnostics.Stopwatch()
                    timer.Start()
                    
                    let! lastRecordedBlock = Db.fetchLastRecordedBlockAsync connection
                    let lastRecordedBlocNumber = HexBigInteger lastRecordedBlock.number
                    let! lastBlockNumberInBlockchain = web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()
                                                       |> Async.AwaitTask

                    do! buildCandleSendCallbackAndWriteToDBAsync connection callback
                                                                 lastBlockNumberInBlockchain
                                                                 lastRecordedBlocNumber
                    //do! Task.Delay(resolutionTime - timer.Elapsed) |> Async.AwaitTask
                    do! Task.Delay(resolutionTime) |> Async.AwaitTask
                printfn "Operation was canceled!"
            with
            | ex -> ex.ToString() |> printfn "%s"
        }

    let firstUniswapExchangeTimestamp =
        DateTime(2018, 11, 2, 10, 33, 56)

    let getTimeSamples (period: DateTime * DateTime) (rate:TimeSpan) =
        let rec inner (start: DateTime, _end) samples =
            if start <= _end then 
                samples
            else
                let newStart = start.Subtract rate
                let newEnd = _end.Subtract rate
                //inner (newStart, _end) ( samples @ [(start, newEnd)])
                inner (newStart, _end) ((start, newEnd) :: samples)
        inner period []
        |> List.rev

    let getCandles connection callback (web3: IWeb3) (cancelToken:CancellationToken) (blockPeriods:(HexBigInteger*HexBigInteger) seq) =
        
        let dateTimeToHex (dateTime:DateTime) = 
            (DateTimeOffset dateTime).ToUnixTimeSeconds()
            |> BigInteger
            |> HexBigInteger
        
        async{
            try
                //let timeSamples = getTimeSamples(startFrom, pancakeLauchDateTimestamp) resolutionTime

                blockPeriods
                |> Seq.takeWhile (
                   fun (startBlockNumber, endBlockNumber) -> 
                            async{
                                try
                                    //let! startBlockNumber =  getBlockNumberByDateTimeAsync false web3 start 
                                    //let! endBlockNumber = getBlockNumberByDateTimeAsync false web3 _end
                                    do! buildCandleSendCallbackAndWriteToDBAsync connection callback 
                                                                                 startBlockNumber
                                                                                 endBlockNumber
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
