namespace RedDuck.Candleswap.Candles

/// Contains C#-friendly wrappers.
module CSharp =
    module Logic =
        open System
        open Nethereum.RPC.Eth.DTOs
        open RedDuck.Candleswap.Candles.Types
        open RedDuck.Candleswap.Candles.Logic
        
        [<Literal>]
        let MaxUInt256StringRepresentation = "115792089237316195423570985008687907853269984665640564039457584007913129639935"

        let private toRefTuple = fun struct (a, b) -> (a, b)

        // Updates candle information with new prices if presented.
        [<CompiledName("PartlyBuildCandle")>]
        let partlyBuildCandle
            (
                transactionsWithReceipts: struct (Transaction * TransactionReceipt) [],
                token0Id,
                token1Id,
                candle: Candle,
                wasRequiredTransactionsInPeriodOfTime,
                firstIterFlag
            ) = 
            partlyBuildCandle 
                (Array.map toRefTuple transactionsWithReceipts) 
                token0Id
                token1Id
                candle
                wasRequiredTransactionsInPeriodOfTime
                firstIterFlag

