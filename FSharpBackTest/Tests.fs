module Tests

open System
open FsCheck
open Program
open Program.Requests
open Program.Logic
open FsUnit.Xunit
open Xunit

type MyGenerators = 
    static member Swap() = 
        {new Arbitrary<Swap>() with
            override _.Generator = gen {
                let generateAmount seed = 
                    Arb.generate<NormalFloat>
                    |> Gen.filter(fun x -> (float x) >= 0.0 && (float x |> decimal) <= (System.Decimal.MaxValue / 2m))
                    |> Gen.eval 1 (Random.StdGen (seed, 1))
                    |> float
                return {
                   id = "0x1fbf001792e8cc747a5cb4aedf8c26b7421147e7"
                   amount0In = 0.0
                   amount0Out = generateAmount 1
                   amount1In = generateAmount 2
                   amount1Out = 0.0
                   timestamp = Arb.generate<int64> |> Gen.eval 1 (Random.StdGen (1, 1))
                }
            }
            override _.Shrinker t = Seq.empty
        }

[<Fact>]
let ``Idempotency of getting swaps in scope``() =
    let getSwapsInScopeIsIdempotent swaps timestampAfter timestampBefore = 
        let firstGet = getSwapsInScope (swaps, timestampAfter, timestampBefore)
        let secondGet = getSwapsInScope (swaps, timestampAfter, timestampBefore)
        List.forall2 (fun first second -> first.timestamp = second.timestamp) firstGet secondGet
    do Arb.register<MyGenerators>() |> ignore
    Check.QuickThrowOnFailure getSwapsInScopeIsIdempotent

[<Fact>]
let ``n swap timestamp always less than n-1 or equal``() =
    let swapsTimestampProperty swaps timestampAfter timestampBefore =
        let swapPairs = getSwapsInScope (swaps, timestampAfter, timestampBefore) |> List.pairwise
        List.forall(fun (n, ``n + 1``) -> n.timestamp <= ``n + 1``.timestamp) swapPairs
    do Arb.register<MyGenerators>() |> ignore
    Check.Quick swapsTimestampProperty

[<Fact>]
let ``Swaps price should not go outside the boundaries of candle low and high``() =
    let candleLowHighProperty swapsSlice res0 res1 uniswapPairId datetime resoultionSeconds = 
        let candle = buildCandle (swapsSlice, res0, res1, uniswapPairId, datetime, resoultionSeconds)
        let k = res1 * res0
        let mutable currentRes0 = res0
        let mutable currentRes1 = res1
        List.forall (fun swap -> currentRes1 <- currentRes1 - ((swap.amount1In + swap.amount1Out) |> decimal)
                                 currentRes0 <- k / currentRes1 
                                 let currentPrice = currentRes1 / currentRes0
                                 currentPrice <= candle.Value.high && currentPrice >= candle.Value.low) swapsSlice
    do Arb.register<MyGenerators>() |> ignore
    Check.Quick candleLowHighProperty

[<Fact>]
let ``Candle open close should not go outside the boundaries of low and high``() = 
    let candleLowHighProperty swapsSlice res0 res1 uniswapPairId datetime resolutionSeconds =
        let candle = buildCandle (swapsSlice, res0, res1, uniswapPairId, datetime, resolutionSeconds)
        candle.Value._open <= candle.Value.high && candle.Value._open >= candle.Value.low && 
        candle.Value.close <= candle.Value.high && candle.Value.close >= candle.Value.low
    do Arb.register<MyGenerators>() |> ignore
    Check.Quick candleLowHighProperty