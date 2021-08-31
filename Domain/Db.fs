namespace Domain

open System
open System.Collections.Generic
open Dapper
open Microsoft.Data.SqlClient
open Types
open Nethereum.Hex.HexTypes

module Db = 
    let private fetchCandlesSql = @"select datetime, resolutionSeconds, pairId, [open] as _open, high, low, [close], volume from candles
        where pairId = @pairId and resolutionSeconds = @resolutionSeconds"

    let private insertCandleSql =
        "insert into candles(datetime, resolutionSeconds, pairId, [open], high, low, [close], volume) "
        + "values (@datetime, @resolutionSeconds, @pairId, @_open, @high, @low, @close, @volume)"

    let private updateCandleSql =
        "update candles set [open] = @_open, high = @high, low = @low, [close] = @close, volume = @volume "
        + "where pairId = @pairId and resolutionSeconds = @resolutionSeconds and datetime = @datetime"

    let private getCandleByDatetimeSql =
        "select datetime, resolutionSeconds, token0Id, token1Id, [open], high, low, [close], volume"
        + "from candles"
        + "where datetime = @datetime"

    let private fetchPairsSql = "select * from pairs"

    let private insertPairSql = "insert into pairs(token0Id, token1Id) values(@token0Id, @token1Id)"

    let private fetchBlocksSql = "select convert(varchar(66), number, 1), convert(varchar(66), timestamp, 1) from blocks"

    let private insertBlockSql = "insert into blocks(number, timestamp) values(convert(varbinary(32), @number, 1), convert(varbinary(32), @timestamp, 1))"

    let private fetchTransactionsSql = 
         "select convert(varchar(66), hash, 1) hash, " +
         "convert(varchar(42), token0Id, 1) token0Id, convert(varchar(42), token1Id, 1) token1Id, convert(varchar(66), amountIn, 1) amountIn, convert(varchar(66), amountOut, 1) amountOut , convert(varchar(66), blockNumber, 1) blockNumber, convert(varchar(66), nonce, 1) nonce " + 
         "from transactions " + 
         "where blockNumber = convert(varbinary(32), @blockNumber, 1)"
    
    let private insertTransactionSql = 
        "insert into transactions(hash, nonce, token0Id, token1Id, amountIn, amountOut, blockNumber) " + 
        "values(convert(varbinary(32), @hash, 1), convert(varbinary(32), @nonce, 1), " +
        "convert(varbinary(20), @token0Id, 1), convert(varbinary(20), @token1Id, 1), convert(varbinary(32), @amountIn, 1), convert(varbinary(32), @amountOut, 1), convert(varbinary(32), @blockNumber, 1))"

    (*let private fetchSwapEventsSql = 
        "select convert(varchar(66), transactionHash, 1) transactionHash, convert(varchar(66), amount0In, 1) amount0In, " +
        "convert(varchar(66), amount1In, 1) amount1In, convert(varchar(66), amount0Out, 1) amount0Out, " +
        "convert(varchar(66), amount1Out, 1) amount1Out " + 
        "from swapEvents " + 
        "where transactionHash = convert(varbinary(32), @transactionHash, 1)"

    let private insertSwapEventSql = 
        "insert into swapEvents(transactionHash, amount0In, amount1In, amount0Out, amount1Out) " +
        "values(convert(varbinary(32), @transactionHash, 1), convert(varbinary(32), @amount0In, 1), " + 
        "convert(varbinary(32), @amount1In, 1), convert(varbinary(32), @amount0Out, 1), convert(varbinary(32), @amount1Out, 1))"*)

    let inline (=>) k v = k, box v

    let private handleHexStringBeforeConvertingToVarbinary (hex:string) = 
        if hex.Length % 2 <> 0 
        then hex.Insert(2, "0")
        else hex

    let private dbQuery<'T> (connection: SqlConnection) (sql: string) (parameters: IDictionary<string, obj> option) =
        match parameters with
        | Some (p) -> connection.QueryAsync<'T>(sql, p)
        | None -> connection.QueryAsync<'T>(sql)

    let private dbExecute (connection: SqlConnection) (sql: string) (data: _) = connection.ExecuteAsync(sql, data)

    let fetchCandles connection pairId (resolutionSeconds: int) =
        async {
            let! candles =
                Async.AwaitTask
                <| dbQuery<DbCandle>
                    connection
                    fetchCandlesSql
                    (Some(
                        dict [ "pairId" => pairId
                               "resolutionSeconds" => resolutionSeconds ]
                    ))

            return candles
        }

    let fetchCandlesTask connection pairId (resolutionSeconds: int) =
        fetchCandles connection pairId resolutionSeconds
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

    let fetchPairsAsync connection = 
        async{
            return! dbQuery<Pair> connection fetchPairsSql None
                    |> Async.AwaitTask
        }

    let addPairAsync connection pair = 
        async{
            let! rowsChanged = dbExecute connection insertPairSql pair
                               |> Async.AwaitTask
            printfn "records added: %i" rowsChanged
        }

    let fetchPairAsync connection token0Id token1Id = 
        async{
            let! pairs = fetchPairsAsync connection
            return pairs |> Seq.tryFind (fun pair -> pair.token0Id = token0Id && pair.token1Id = token1Id)
        }

    let fetchPairOrCreateNewIfNotExists connection token0Id token1Id = 
        async{
            let! pair = fetchPairAsync connection token0Id token1Id
            match pair with
            | Some pair -> return pair
            | None -> let pair = {id = 0L; token0Id = token0Id; token1Id = token1Id}
                      do! addPairAsync connection pair
                      return pair
        }

    let addPairsIfNotExistAsync connection pairs = 
        async{
            let! pairsFromDb = fetchPairsAsync connection
            for pair in pairs do
                let pairFromDb = 
                    pairsFromDb
                    |> Seq.tryFind(fun p -> p.token0Id = pair.token0Id && p.token1Id = pair.token1Id)
                if pairFromDb.IsNone
                then do! addPairAsync connection pair
        }

    let fetchBlocksAsync connection = 
        async{
            return! dbQuery<Block> connection fetchBlocksSql None
                    |> Async.AwaitTask
        }

    let addBlockAsync connection (block:Block) =
        async{
            let record = {number  = handleHexStringBeforeConvertingToVarbinary block.number
                          timestamp = handleHexStringBeforeConvertingToVarbinary block.timestamp}
            let! rowsChanged = dbExecute connection insertBlockSql record
                               |> Async.AwaitTask
            printfn "records added: %i" rowsChanged
        }

    let fetchTransactionsAsync connection (blockNumber:HexBigInteger) = 
        async {
            let param = handleHexStringBeforeConvertingToVarbinary blockNumber.HexValue
            let! transactions =
                Async.AwaitTask
                <| dbQuery<DbTransaction>
                    connection
                    fetchTransactionsSql
                    (Some(dict [ "blockNumber" => param]))
            return transactions
        }

    let addTransactionAsync connection (transaction:DbTransaction) = 
        async {
            let record = {hash = handleHexStringBeforeConvertingToVarbinary transaction.hash
                          token0Id = handleHexStringBeforeConvertingToVarbinary transaction.token0Id
                          token1Id = handleHexStringBeforeConvertingToVarbinary transaction.token1Id
                          amountIn = handleHexStringBeforeConvertingToVarbinary transaction.amountIn
                          amountOut = handleHexStringBeforeConvertingToVarbinary transaction.amountOut
                          blockNumber = handleHexStringBeforeConvertingToVarbinary transaction.blockNumber
                          nonce = handleHexStringBeforeConvertingToVarbinary transaction.nonce}
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection insertTransactionSql record

            printfn "records added: %i" rowsChanged
        }

    (*let fetchSwapEvents connection transactionHash = 
        let param = handleHexStringBeforeConvertingToVarbinary transactionHash
        async {
            let! swapEvents =
                Async.AwaitTask
                <| dbQuery<DbSwapEvent>
                    connection
                    fetchSwapEventsSql
                    (Some(dict [ "transactionHash" => param]))
            return swapEvents
        }

    let addSwapEvent connection (swapEvent:DbSwapEvent) = 
        async {
            let record = {transactionHash = handleHexStringBeforeConvertingToVarbinary swapEvent.transactionHash
                          amount0In = handleHexStringBeforeConvertingToVarbinary swapEvent.amount0In
                          amount1In = handleHexStringBeforeConvertingToVarbinary swapEvent.amount1In
                          amount0Out = handleHexStringBeforeConvertingToVarbinary swapEvent.amount0Out
                          amount1Out = handleHexStringBeforeConvertingToVarbinary swapEvent.amount1Out}
            let! rowsChanged =
                Async.AwaitTask
                <| dbExecute connection insertSwapEventSql record

            printfn "records added: %i" rowsChanged
        }*)