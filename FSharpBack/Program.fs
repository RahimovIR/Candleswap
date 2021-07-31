open System
open System.Data.SQLite
open Nethereum.Web3
open RedDuck.Candleswap.Candles

[<EntryPoint>]
let main args =
    let connectionString = args.[0]
    let connection = new SQLiteConnection(connectionString)
    let _ = Db.fetchCandles connection "ads" "asd" 10 |> Async.RunSynchronously
    0
