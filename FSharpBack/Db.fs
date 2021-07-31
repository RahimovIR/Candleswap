namespace RedDuck.Candleswap.Candles

open System
open System.Data.SQLite
open System.Collections.Generic
open Dapper
open RedDuck.Candleswap.Candles.Types

module Db = 
    let private fetchCandlesSql = @"select datetime, resolutionSeconds, token0Id, token1Id, open as _open, high, low, close, volume from candles
        where token0Id = @token0Id and token1Id = @token1Id and resolutionSeconds = @resolutionSeconds"

    let private insertCandleSql =
        "insert into candles(datetime, resolutionSeconds, token0Id, token1Id, open, high, low, close, volume) "
        + "values (@datetime, @resolutionSeconds, @token0Id, @token1Id, @_open, @high, @low, @close, @volume)"

    let private updateCandleSql =
        "update candles set open = @_open, high = @high, low = @low, close = @close, volume = @volume "
        + "where token0Id = @token0Id and token1Id = @token1Id and resolutionSeconds = @resolutionSeconds and datetime = @datetime"

    let private getCandleByDatetimeSql =
        "select datetime, resolutionSeconds, token0Id, token1Id, open, high, low, close, volume"
        + "from candles"
        + "where datetime = @datetime"

    let inline (=>) k v = k, box v

    let private dbQuery<'T> (connection: SQLiteConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
        match parameters with
        | Some (p) -> connection.QueryAsync<'T>(sql, p)
        | None -> connection.QueryAsync<'T>(sql)

    let private dbExecute (connection: SQLiteConnection) (sql: string) (data: _) = connection.ExecuteAsync(sql, data)

    let fetchCandles connection (token0Id: string) (token1Id:string) (resolutionSeconds: int) =
        async {
            let! candles =
                Async.AwaitTask
                <| dbQuery<DbCandle>
                    connection
                    fetchCandlesSql
                    (Some(
                        dict [ "token0Id" => token0Id
                               "token1Id" => token1Id
                               "resolutionSeconds" => resolutionSeconds ]
                    ))

            return candles
        }

    let fetchCandlesTask connection (token0Id: string) (token1Id:string) (resolutionSeconds: int) =
        fetchCandles connection token0Id token1Id resolutionSeconds
        |> Async.StartAsTask

    let addCandle connection candle =
        async {
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection insertCandleSql candle

            printfn "records added: %i" rowsChanged
        }

    let updateCandle connection candle =
        async {
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection updateCandleSql candle

            printfn "records added: %i" rowsChanged
        }

    let getCandleByDatetime connection datetime =
        async {
            let queryParams = Some <| dict [ "datetime" => datetime ]

            let! candle =
                Async.AwaitTask
                <| dbQuery connection getCandleByDatetimeSql queryParams

            return candle
        }
