open System
open Nethereum.Web3
open RedDuck.Candleswap.Candles

[<EntryPoint>]
let main _ =
    let pairId = "0x8ad599c3a0ff1de082011efddc58f1908eb6e6d8"
    let resolutionTime = new TimeSpan(0, 5, 0)
    let web3 =
        new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e")
    
    (pairId, (fun c -> printfn "%A" c), resolutionTime, web3) |> Logic.getCandles
    0
