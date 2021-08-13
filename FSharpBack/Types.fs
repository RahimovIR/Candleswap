namespace RedDuck.Candleswap.Candles

open System
open System.Numerics
open Nethereum.Util

module Types =
    type Candle =
        { _open: BigDecimal
          high: BigDecimal
          low: BigDecimal
          close: BigDecimal
          volume: uint }
    
    [<CLIMutable>]
    type DbCandle =
        { datetime: int64
          resolutionSeconds: int
          pairId:int64
          _open: string
          high: string
          low: string
          close: string
          volume: int }

    type Pair =
        { id: int64
          token0Id: string
          token1Id: string }