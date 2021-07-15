namespace RedDuck.Candleswap.Candles

open System
open System.Numerics
open FSharp.Data.GraphQL
open Newtonsoft.Json.Linq

module Requests =
    let swapsQuery id =
        $"""query q {{
               swaps(orderBy: timestamp, orderDirection: desc,
                     where:{{ pair: "{id}" }})
                {{
                    amount0In
                    amount0Out
                    amount1In
                    amount1Out
                    timestamp
                }}
               }}"""

    let pairInfoQuery id =
        $"""query q {{
               pair(id: "{id}"){{
                   reserve0
                   reserve1
                   token0Price
                   token1Price
                   token0{{
                       id
                   }}
                   token1{{
                       id
                   }}
               }}
              }}"""

    let poolInfoQuery id =
        $"""query q {{
            pool(id: "{id}"){{
                token0{{
                    id
                }}
                token1{{
                    id
                }}
            }}
           }}"""


    let requestMaker serverUrl query =
        use connection = new GraphQLClientConnection()

        let request : GraphQLRequest =
            { Query = query
              Variables = [||]
              ServerUrl = serverUrl
              HttpHeaders = [||]
              OperationName = Some "q" }

        GraphQLClient.sendRequest connection request

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

    type PoolInfo = { token0Id: string; token1Id: string }

    let mapSwaps (token: JToken Option) =
        let mapper (token: JProperty) =
            token.Value.["swaps"]
            |> Seq.map
                (fun x ->
                    { id = (string x.["id"])
                      amount0In = (float x.["amount0In"])
                      amount0Out = (float x.["amount0Out"])
                      amount1In = (float x.["amount1In"])
                      amount1Out = (float x.["amount1Out"])
                      timestamp = (int64 x.["timestamp"]) })

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> List.ofSeq
            |> Some
        | None -> None

    let mapPairInfo (token: JToken Option) =
        let mapper (token: JProperty) =
            let info = token.Value.["pair"]

            { reserve0 = (info.Value<decimal>("reserve0") |> BigInteger)
              reserve1 = (info.Value<decimal>("reserve1") |> BigInteger)
              price0 = (float info.["token0Price"])
              price1 = (float info.["token1Price"])
              token0Id = info.["token0"].["id"].ToString()
              token1Id = info.["token1"].["id"].ToString() }

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> Some
        | None -> None

    let mapPoolInfo (token: JToken Option) =
        let mapper (token: JProperty) =
            let info = token.Value.["pool"]

            { token0Id = info.["token0"].["id"].ToString()
              token1Id = info.["token1"].["id"].ToString() }

        match token with
        | Some token ->
            token.Children<JProperty>()
            |> Seq.last
            |> mapper
            |> Some
        | None -> None

    let deserialize (data: string) =
        if String.IsNullOrWhiteSpace(data) then
            None
        else
            data |> JToken.Parse |> Some

    let allPr x = printfn "%A" x

    let uniswapV2 =
        "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v2"

    let uniswapV3 =
        "https://api.thegraph.com/subgraphs/name/uniswap/uniswap-v3"

    let takeSwaps idPair =
        idPair
        |> swapsQuery
        |> requestMaker uniswapV2
        |> deserialize
        |> mapSwaps

    let takePairInfo idPair =
        idPair
        |> pairInfoQuery
        |> requestMaker uniswapV2
        |> deserialize
        |> mapPairInfo

    let takePoolInfo idPair =
        idPair
        |> poolInfoQuery
        |> requestMaker uniswapV3
        |> deserialize
        |> mapPoolInfo

