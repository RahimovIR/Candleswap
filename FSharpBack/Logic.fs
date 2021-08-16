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

module Logic =
    let maxUInt256StringRepresentation =
        "115792089237316195423570985008687907853269984665640564039457584007913129639935"

    // Updates candle information with new open, close, high and low prices if presented.
    let partlyBuildCandle
        (transactionsWithReceipts: (Transaction * TransactionReceipt) [])
        token0Id
        token1Id
        (candle: Candle)
        wasRequiredTransactionsInPeriodOfTime
        firstIterFlag =
        let tryGetTransactionInfo addresses infoFunc (transaction: Transaction) =
            if List.contains transaction.To addresses 
            then Some infoFunc
            else None
        
        /// Returns Some function to obtain transaction information if transaction recipient 
        /// matches any known.
        let transactionData transaction receipt = 
          [ tryGetTransactionInfo SwapRouterV3.addresses SwapRouterV3.getInfoFromRouter
            tryGetTransactionInfo SwapRouterV2.addresses SwapRouterV2.getInfoFromRouter
            tryGetTransactionInfo ExchangeV1.addresses ExchangeV1.getInfoFromExchange ] 
          |> List.map (fun f -> f transaction)
          |> List.tryFind Option.isSome 
          |> Option.get
          |> Option.map (fun f -> f transaction receipt)

        /// Extended information about transactions to Uniswap contracts.
        let actualTransactionsData = 
            transactionsWithReceipts
            |> Array.map (fun (t, r) -> transactionData t r)
            |> Array.choose id
            |> Array.filter (fun (inAddr, outAddr, _, _) -> (inAddr, outAddr) = (token0Id, token1Id))

        let currentPrice (_, _, amountIn, amountOut) = BigDecimal(amountIn, 0) / BigDecimal(amountOut, 0)
        
        let openPrice = if actualTransactionsData.Length > 0 then (Array.last >> currentPrice) actualTransactionsData
                        else candle._open
        let closePrice = if actualTransactionsData.Length > 0 then (Array.head >> currentPrice) actualTransactionsData
                         else candle.close

        let high, low, volume = 
            let folder (high, low, volume) price =
                (if high > price then high else price),
                (if low < price then low else price),
                (volume + 1u)
            actualTransactionsData
            |> Seq.map currentPrice
            |> Seq.fold folder (candle.high, candle.low, candle.volume)

        let candle = { close = closePrice
                       _open = openPrice
                       low = low
                       high = high
                       volume = volume }
        
        let anyTransactionInPeriod = 
            not (Array.isEmpty actualTransactionsData) || wasRequiredTransactionsInPeriodOfTime
        let firstIterFlag = if anyTransactionInPeriod then false else firstIterFlag

        candle, anyTransactionInPeriod, firstIterFlag

    let buildCandleAsync
        (currentTime: DateTimeOffset)
        (resolutionTime: TimeSpan)
        resolutionTimeAgoUnix
        pair
        (web3: IWeb3)
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

                let (_candle, _wasRequiredTransactionsInPeriodOfTime, _firstIterFlag) =
                    partlyBuildCandle
                        successfulSwapTransactionsWithReceipts
                        pair.token0Id
                        pair.token1Id
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
                        { datetime = currentTime.ToUnixTimeSeconds()
                          resolutionSeconds = (int) resolutionTime.TotalSeconds
                          pairId = pair.id 
                          _open = candle._open.ToString()
                          high = candle.high.ToString()
                          low = candle.low.ToString()
                          close = candle.close.ToString()
                          volume = (int)candle.volume }
                else
                    None

            return dbCandle
        }

    let sendCallbackAndWriteToDB 
        connection 
        (candle: DbCandle option) 
        (currentTime: DateTimeOffset) 
        (resolutionTimeAgo: DateTimeOffset) 
        (pair:Pair)
        callback =
        match candle with
        | Some candle ->
            callback (
                $"token0Id:{pair.token0Id}\ntoken1Id:{pair.token1Id}\nresolutionSeconds:{candle.resolutionSeconds}\n"
                + $"datetime:{candle.datetime}\n_open:{candle._open}\nlow:{candle.low}\nhigh:{candle.high}\n"
                + $"close:{candle.close}\nvolume:{candle.volume}"
            )

            Db.addCandle connection candle
        | None -> async{ callback $"No swaps\nfrom:{resolutionTimeAgo.DateTime}\nto:{currentTime.DateTime}"}

    let buildCandleSendCallbackAndWriteToDBAsync
        connection
        (resolutionTime: TimeSpan)
        pair
        callback
        (web3: IWeb3)
        (currentTime: DateTimeOffset)
        =
        async {
            let resolutionTimeAgo = currentTime.Subtract(resolutionTime)

            let resolutionTimeAgoUnix =
                resolutionTimeAgo.ToUnixTimeSeconds()
                |> BigInteger

            let! dbCandle = buildCandleAsync currentTime resolutionTime resolutionTimeAgoUnix pair web3
            return! sendCallbackAndWriteToDB connection dbCandle currentTime resolutionTimeAgo pair callback
        }

    let getCandle connection pair callback (resolutionTime: TimeSpan) (web3: IWeb3) cancelToken =
        let timer =
            new System.Timers.Timer(resolutionTime.TotalMilliseconds)

        let candlesHandler = new ElapsedEventHandler(fun _ _ ->
                try 
                    let computation = DateTime.Now.ToUniversalTime()
                                      |> DateTimeOffset
                                      |> buildCandleSendCallbackAndWriteToDBAsync connection resolutionTime pair callback web3
                    Async.RunSynchronously(computation, ?cancellationToken = Some(cancelToken))
                with
                | :? System.OperationCanceledException -> printfn "Operation was canceled!")

        timer.Elapsed.AddHandler(candlesHandler)
        timer.Start()

        while true do
            ()

    let firstUniswapExchangeTimestamp =
        DateTime(2018, 11, 2, 10, 33, 56).ToUniversalTime()

    let getTimeSamples (period: DateTime * DateTime) rate =
        let rec inner (a: DateTime, b) samples =
            if a >= b then 
                samples
            else
                inner (a.Add rate, b) (DateTimeOffset a :: samples)
        inner period []

    let getCandles connection pair callback (resolutionTime: TimeSpan) (web3: IWeb3) cancelToken =
        try
            let b = DateTime.Now.ToUniversalTime()
            let a = firstUniswapExchangeTimestamp
            for t in getTimeSamples (a, b) resolutionTime do
                let computation = buildCandleSendCallbackAndWriteToDBAsync 
                                      connection resolutionTime pair callback web3 t
                Async.RunSynchronously(computation, ?cancellationToken = Some(cancelToken))
        
            callback "Processing completed successfully"
        with
        | :? System.OperationCanceledException -> printfn "Operation was canceled!"
