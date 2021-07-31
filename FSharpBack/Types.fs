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
        { datetime: DateTime
          resolutionSeconds: int
          token0Id: string
          token1Id: string
          _open: string
          high: string
          low: string
          close: string
          volume: uint }

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