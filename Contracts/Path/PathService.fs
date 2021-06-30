namespace Contracts.Path

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts.ContractHandlers
open Nethereum.Contracts
open System.Threading
open Contracts.Path.ContractDefinition


    type PathService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, pathDeployment: PathDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<PathDeployment>().SendRequestAndWaitForReceiptAsync(pathDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, pathDeployment: PathDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<PathDeployment>().SendRequestAsync(pathDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, pathDeployment: PathDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = PathService.DeployContractAndWaitForReceiptAsync(web3, pathDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new PathService(web3, receipt.ContractAddress);
            }
    
        member this.DecodeFirstPoolQueryAsync(decodeFirstPoolFunction: DecodeFirstPoolFunction, ?blockParameter: BlockParameter): Task<DecodeFirstPoolOutputDTO> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryDeserializingToObjectAsync<DecodeFirstPoolFunction, DecodeFirstPoolOutputDTO>(decodeFirstPoolFunction, blockParameterVal)
            
        member this.GetFirstPoolQueryAsync(getFirstPoolFunction: GetFirstPoolFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetFirstPoolFunction, byte[]>(getFirstPoolFunction, blockParameterVal)
            
        member this.HasMultiplePoolsQueryAsync(hasMultiplePoolsFunction: HasMultiplePoolsFunction, ?blockParameter: BlockParameter): Task<bool> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<HasMultiplePoolsFunction, bool>(hasMultiplePoolsFunction, blockParameterVal)
            
        member this.NumPoolsQueryAsync(numPoolsFunction: NumPoolsFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<NumPoolsFunction, BigInteger>(numPoolsFunction, blockParameterVal)
            
        member this.SkipTokenQueryAsync(skipTokenFunction: SkipTokenFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<SkipTokenFunction, byte[]>(skipTokenFunction, blockParameterVal)
            
    

