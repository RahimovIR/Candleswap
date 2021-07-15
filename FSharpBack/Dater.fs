namespace RedDuck.Candleswap.Candles

open System
open System.Collections.Generic
open System.Numerics
open Nethereum.Web3
open Nethereum.Hex.HexTypes

module Dater =
    type BlockNumberTimestamp =
        { number: HexBigInteger
          timestamp: HexBigInteger }

    let getBlockNumberAndTimestampAsync
        (savedBlocks: Dictionary<HexBigInteger, HexBigInteger>)
        (web3: Web3)
        (blockNumber: HexBigInteger)
        =
        async {
            let timestamp = ref (HexBigInteger "0")

            if savedBlocks.TryGetValue(blockNumber, timestamp) then
                return
                    { number = blockNumber
                      timestamp = timestamp.Value }
            else
                let! block =
                    Async.AwaitTask
                    <| web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(blockNumber)

                savedBlocks.Add(blockNumber, block.Timestamp)

                return
                    { number = blockNumber
                      timestamp = block.Timestamp }
        }

    let isBestBlock date predictedBlockTime previousBlockTime nextBlockTime ifBlockAfterDate =
        match ifBlockAfterDate with
        | true ->
            if predictedBlockTime < date then
                false
            else if predictedBlockTime >= date
                    && previousBlockTime < date then
                true
            else
                false
        | false ->
            if predictedBlockTime > date then
                false
            else if predictedBlockTime <= date && nextBlockTime > date then
                true
            else
                false

    let getNextPredictedBlockNumber currentPredictedBlockNumber skip =
        let nextPredictedBlockNumber = currentPredictedBlockNumber + skip

        if nextPredictedBlockNumber <= 1I then
            1I
        else
            nextPredictedBlockNumber

    let rec findBestBlock date predictedBlock ifBlockAfterDate blockTime savedBlocks checkedBlocks web3 =
        async {
            let! previousPredictedBlock =
                predictedBlock.number.Value - 1I
                |> HexBigInteger
                |> getBlockNumberAndTimestampAsync savedBlocks web3

            let! nextPredictedBlock =
                predictedBlock.number.Value + 1I
                |> HexBigInteger
                |> getBlockNumberAndTimestampAsync savedBlocks web3

            if isBestBlock
                date
                predictedBlock.timestamp.Value
                previousPredictedBlock.timestamp.Value
                nextPredictedBlock.timestamp.Value
                ifBlockAfterDate then
                return predictedBlock
            else
                let difference = date - predictedBlock.timestamp.Value

                let mutable skip =
                    (float difference) / blockTime |> Math.Ceiling

                if skip = 0.0 then
                    skip <- if difference < 0I then -1.0 else 1.0

                let! nextPredictedBlock =
                    getNextPredictedBlockNumber predictedBlock.number.Value (BigInteger skip)
                    |> HexBigInteger
                    |> getBlockNumberAndTimestampAsync savedBlocks web3

                let newBlockTime =
                    (predictedBlock.timestamp.Value
                     - nextPredictedBlock.timestamp.Value)
                    / (predictedBlock.number.Value
                       - nextPredictedBlock.number.Value)
                    |> float
                    |> Math.Abs

                return!
                    findBestBlock date nextPredictedBlock ifBlockAfterDate newBlockTime savedBlocks checkedBlocks web3
        }

    let getBlockByDateAsync ifBlockAfterDate (web3: Web3) date =
        async {
            let savedBlocks =
                new Dictionary<HexBigInteger, HexBigInteger>()

            let checkedBlocks = new List<BigInteger>()

            let! latestBlockNumber =
                web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()
                |> Async.AwaitTask

            let! latestBlock = getBlockNumberAndTimestampAsync savedBlocks web3 latestBlockNumber
            let firstBlockNumber = HexBigInteger 1I
            let! firstBlock = getBlockNumberAndTimestampAsync savedBlocks web3 firstBlockNumber

            let blockTime =
                (float (
                    latestBlock.timestamp.Value
                    - firstBlock.timestamp.Value
                ))
                / (float (latestBlock.number.Value - 1I))

            if date <= firstBlock.timestamp.Value then
                return firstBlock
            else if date >= latestBlock.timestamp.Value then
                return latestBlock
            else
                let! predictedBlock =
                    (float (date - firstBlock.timestamp.Value))
                    / blockTime
                    |> Math.Ceiling
                    |> BigInteger
                    |> HexBigInteger
                    |> getBlockNumberAndTimestampAsync savedBlocks web3

                return! findBestBlock date predictedBlock ifBlockAfterDate blockTime savedBlocks checkedBlocks web3
        }

    let convertToUnixTimestamp (date: DateTime) =
        let origin =
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)

        let diff = date.ToUniversalTime() - origin
        Math.Floor(diff.TotalSeconds)



