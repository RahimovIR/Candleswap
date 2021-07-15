namespace RedDuck.Candleswap.Candles

open System
open Nethereum.Util

module Types =
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
