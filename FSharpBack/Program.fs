open System
open Nethereum.Web3
open RedDuck.Candleswap.Candles
open System.Data.SqlClient

[<EntryPoint>]
let main args =
    let connectionString = args.[0]
    let connection = new SqlConnection(connectionString)
    let _ = Db.fetchCandles connection 1 10 |> Async.RunSynchronously
    0
