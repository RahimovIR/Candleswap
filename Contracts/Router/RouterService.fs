namespace Contracts.Router

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
open Contracts.Router.ContractDefinition


    type RouterService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, routerDeployment: RouterDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<RouterDeployment>().SendRequestAndWaitForReceiptAsync(routerDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, routerDeployment: RouterDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<RouterDeployment>().SendRequestAsync(routerDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, routerDeployment: RouterDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = RouterService.DeployContractAndWaitForReceiptAsync(web3, routerDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new RouterService(web3, receipt.ContractAddress);
            }
    
        member this.WETH9QueryAsync(wETH9Function: WETH9Function, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<WETH9Function, string>(wETH9Function, blockParameterVal)
            
        member this.ExactInputRequestAsync(exactInputFunction: ExactInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(exactInputFunction);
        
        member this.ExactInputRequestAndWaitForReceiptAsync(exactInputFunction: ExactInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(exactInputFunction, cancellationTokenSourceVal);
        
        member this.ExactInputSingleRequestAsync(exactInputSingleFunction: ExactInputSingleFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(exactInputSingleFunction);
        
        member this.ExactInputSingleRequestAndWaitForReceiptAsync(exactInputSingleFunction: ExactInputSingleFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(exactInputSingleFunction, cancellationTokenSourceVal);
        
        member this.ExactOutputRequestAsync(exactOutputFunction: ExactOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(exactOutputFunction);
        
        member this.ExactOutputRequestAndWaitForReceiptAsync(exactOutputFunction: ExactOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(exactOutputFunction, cancellationTokenSourceVal);
        
        member this.ExactOutputSingleRequestAsync(exactOutputSingleFunction: ExactOutputSingleFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(exactOutputSingleFunction);
        
        member this.ExactOutputSingleRequestAndWaitForReceiptAsync(exactOutputSingleFunction: ExactOutputSingleFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(exactOutputSingleFunction, cancellationTokenSourceVal);
        
        member this.FactoryQueryAsync(factoryFunction: FactoryFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<FactoryFunction, string>(factoryFunction, blockParameterVal)
            
        member this.MulticallRequestAsync(multicallFunction: MulticallFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(multicallFunction);
        
        member this.MulticallRequestAndWaitForReceiptAsync(multicallFunction: MulticallFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(multicallFunction, cancellationTokenSourceVal);
        
        member this.RefundETHRequestAsync(refundETHFunction: RefundETHFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(refundETHFunction);
        
        member this.RefundETHRequestAndWaitForReceiptAsync(refundETHFunction: RefundETHFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(refundETHFunction, cancellationTokenSourceVal);
        
        member this.SelfPermitRequestAsync(selfPermitFunction: SelfPermitFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(selfPermitFunction);
        
        member this.SelfPermitRequestAndWaitForReceiptAsync(selfPermitFunction: SelfPermitFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(selfPermitFunction, cancellationTokenSourceVal);
        
        member this.SelfPermitAllowedRequestAsync(selfPermitAllowedFunction: SelfPermitAllowedFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(selfPermitAllowedFunction);
        
        member this.SelfPermitAllowedRequestAndWaitForReceiptAsync(selfPermitAllowedFunction: SelfPermitAllowedFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(selfPermitAllowedFunction, cancellationTokenSourceVal);
        
        member this.SelfPermitAllowedIfNecessaryRequestAsync(selfPermitAllowedIfNecessaryFunction: SelfPermitAllowedIfNecessaryFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(selfPermitAllowedIfNecessaryFunction);
        
        member this.SelfPermitAllowedIfNecessaryRequestAndWaitForReceiptAsync(selfPermitAllowedIfNecessaryFunction: SelfPermitAllowedIfNecessaryFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(selfPermitAllowedIfNecessaryFunction, cancellationTokenSourceVal);
        
        member this.SelfPermitIfNecessaryRequestAsync(selfPermitIfNecessaryFunction: SelfPermitIfNecessaryFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(selfPermitIfNecessaryFunction);
        
        member this.SelfPermitIfNecessaryRequestAndWaitForReceiptAsync(selfPermitIfNecessaryFunction: SelfPermitIfNecessaryFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(selfPermitIfNecessaryFunction, cancellationTokenSourceVal);
        
        member this.SweepTokenRequestAsync(sweepTokenFunction: SweepTokenFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(sweepTokenFunction);
        
        member this.SweepTokenRequestAndWaitForReceiptAsync(sweepTokenFunction: SweepTokenFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(sweepTokenFunction, cancellationTokenSourceVal);
        
        member this.SweepTokenWithFeeRequestAsync(sweepTokenWithFeeFunction: SweepTokenWithFeeFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(sweepTokenWithFeeFunction);
        
        member this.SweepTokenWithFeeRequestAndWaitForReceiptAsync(sweepTokenWithFeeFunction: SweepTokenWithFeeFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(sweepTokenWithFeeFunction, cancellationTokenSourceVal);
        
        member this.UniswapV3SwapCallbackRequestAsync(uniswapV3SwapCallbackFunction: UniswapV3SwapCallbackFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(uniswapV3SwapCallbackFunction);
        
        member this.UniswapV3SwapCallbackRequestAndWaitForReceiptAsync(uniswapV3SwapCallbackFunction: UniswapV3SwapCallbackFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(uniswapV3SwapCallbackFunction, cancellationTokenSourceVal);
        
        member this.UnwrapWETH9RequestAsync(unwrapWETH9Function: UnwrapWETH9Function): Task<string> =
            this.ContractHandler.SendRequestAsync(unwrapWETH9Function);
        
        member this.UnwrapWETH9RequestAndWaitForReceiptAsync(unwrapWETH9Function: UnwrapWETH9Function, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(unwrapWETH9Function, cancellationTokenSourceVal);
        
        member this.UnwrapWETH9WithFeeRequestAsync(unwrapWETH9WithFeeFunction: UnwrapWETH9WithFeeFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(unwrapWETH9WithFeeFunction);
        
        member this.UnwrapWETH9WithFeeRequestAndWaitForReceiptAsync(unwrapWETH9WithFeeFunction: UnwrapWETH9WithFeeFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(unwrapWETH9WithFeeFunction, cancellationTokenSourceVal);
        
    

