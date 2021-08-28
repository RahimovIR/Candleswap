open System
open Nethereum.Web3
open RedDuck.Candleswap.Candles
open System.Data.SqlClient
open System.Threading

[<EntryPoint>]
let main args =
    let connectionString = args.[0]
    let connection = new SqlConnection(connectionString)
    //let connection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=candleswap;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False;MultipleActiveResultSets=True")
    connection.Open()
    let period = (float)args.[1]
    let web3 = Web3(args.[2])
    let startFrom = DateTimeOffset.UtcNow.DateTime


    Logic.getCandles connection (fun candle -> printfn "candle received" ) (TimeSpan.FromSeconds(period)) web3 CancellationToken.None startFrom
    |> Async.RunSynchronously
    0
