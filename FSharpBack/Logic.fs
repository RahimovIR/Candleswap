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

module Logic =
    let maxUInt256StringRepresentation =
        "115792089237316195423570985008687907853269984665640564039457584007913129639935"

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
                let (tokenInAddress, tokenOutAddress, amountIn, amountOut) = 
                    if transaction.To = SwapRouterV3.routerAddress 
                    then SwapRouterV3.getInfoFromRouter transaction receipt
                    else if transaction.To = SwapRouterV2.router02Address ||
                            transaction.To = SwapRouterV2.router01Address
                    then SwapRouterV2.getInfoFromRouter transaction receipt
                    else if transaction.To = ExchangeV1.exchangeAddress
                    then ExchangeV1.getInfoFromExchange transaction receipt
                    else ("", "", 0I, 0I)

                if token0Id = tokenInAddress && token1Id = tokenOutAddress then
                    _wasRequiredTransactionsInPeriodOfTime <- true
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

            Db.addCandle candle
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

        while currentTime >= firstUniswapExchangeTimestamp do
            currentTime
            |> DateTimeOffset
            |> buildCandleSendCallbackAndWriteToDBAsync resolutionTime pairId callback web3
            |> Async.RunSynchronously
            currentTime <- currentTime.Subtract(resolutionTime)
        callback "Processing completed successfully"


