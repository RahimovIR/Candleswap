namespace RedDuck.Candleswap.Candles

open System
open System.Data.SQLite
open System.Collections.Generic
open Dapper
open RedDuck.Candleswap.Candles.Types

module Db =
    
    let private databaseFilename = __SOURCE_DIRECTORY__ + @"\Database\candles.db"
    let defaultConnectionString = sprintf "Data Source=%s;Version=3;" databaseFilename

    let internal connection (connectionString: string) = new SQLiteConnection(connectionString)

    let private fetchCandlesSql = @"select datetime, resolutionSeconds, uniswapPairId, open as _open, high, low, close, volume from candles
        where uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds"

    let private insertCandleSql =
        "insert into candles(datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume) "
        + "values (@datetime, @resolutionSeconds, @uniswapPairId, @_open, @high, @low, @close, @volume)"

    let private updateCandleSql =
        "update candles set open = @_open, high = @high, low = @low, close = @close, volume = @volume) "
        + "values uniswapPairId = @uniswapPairId and resolutionSeconds = @resolutionSeconds and datetime = @datetime)"

    let private getCandleByDatetimeSql =
        "select datetime, resolutionSeconds, uniswapPairId, open, high, low, close, volume"
        + "from candles"
        + "where datetime = @datetime"

    let inline (=>) k v = k, box v

    let private dbQuery<'T> (connection: SQLiteConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
        match parameters with
        | Some (p) -> connection.QueryAsync<'T>(sql, p)
        | None -> connection.QueryAsync<'T>(sql)

    let private dbExecute (connection: SQLiteConnection) (sql: string) (data: _) = connection.ExecuteAsync(sql, data)

    let fetchCandles connection (uniswapPairId: string) (resolutionSeconds: int) =
        async {
            let query = dbQuery<DbCandle> connection fetchCandlesSql
            let task = query ^ Some(dict [ 
                "uniswapPairId" => uniswapPairId
                "resolutionSeconds" => resolutionSeconds ])
            try
                let! candles = Async.AwaitTask task
                return candles
            with e -> 
                raise e
                return Seq.empty
        }

    let fetchCandlesTask connection (uniswapPairId: string) (resolutionSeconds: int) =
        Async.StartAsTask
        <| fetchCandles connection uniswapPairId resolutionSeconds

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

    let getCandleByDatetime connection  datetime =
        async {
            let queryParams = Some <| dict [ "datetime" => datetime ]

            let! candle =
                Async.AwaitTask
                <| dbQuery connection getCandleByDatetimeSql queryParams

            return candle
        }
