namespace Contracts.PancakeV2Router

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
open Contracts.PancakeV2Router.ContractDefinition


    type PancakeV2RouterService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, pancakeV2RouterDeployment: PancakeV2RouterDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<PancakeV2RouterDeployment>().SendRequestAndWaitForReceiptAsync(pancakeV2RouterDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, pancakeV2RouterDeployment: PancakeV2RouterDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<PancakeV2RouterDeployment>().SendRequestAsync(pancakeV2RouterDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, pancakeV2RouterDeployment: PancakeV2RouterDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = PancakeV2RouterService.DeployContractAndWaitForReceiptAsync(web3, pancakeV2RouterDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new PancakeV2RouterService(web3, receipt.ContractAddress);
            }
    
        member this.WETHQueryAsync(wETHFunction: WETHFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<WETHFunction, string>(wETHFunction, blockParameterVal)
            
        member this.AddLiquidityRequestAsync(addLiquidityFunction: AddLiquidityFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(addLiquidityFunction);
        
        member this.AddLiquidityRequestAndWaitForReceiptAsync(addLiquidityFunction: AddLiquidityFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityFunction, cancellationTokenSourceVal);
        
        member this.AddLiquidityETHRequestAsync(addLiquidityETHFunction: AddLiquidityETHFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(addLiquidityETHFunction);
        
        member this.AddLiquidityETHRequestAndWaitForReceiptAsync(addLiquidityETHFunction: AddLiquidityETHFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityETHFunction, cancellationTokenSourceVal);
        
        member this.FactoryQueryAsync(factoryFunction: FactoryFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<FactoryFunction, string>(factoryFunction, blockParameterVal)
            
        member this.GetAmountInQueryAsync(getAmountInFunction: GetAmountInFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetAmountInFunction, BigInteger>(getAmountInFunction, blockParameterVal)
            
        member this.GetAmountOutQueryAsync(getAmountOutFunction: GetAmountOutFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetAmountOutFunction, BigInteger>(getAmountOutFunction, blockParameterVal)
            
        member this.GetAmountsInQueryAsync(getAmountsInFunction: GetAmountsInFunction, ?blockParameter: BlockParameter): Task<List<BigInteger>> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetAmountsInFunction, List<BigInteger>>(getAmountsInFunction, blockParameterVal)
            
        member this.GetAmountsOutQueryAsync(getAmountsOutFunction: GetAmountsOutFunction, ?blockParameter: BlockParameter): Task<List<BigInteger>> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetAmountsOutFunction, List<BigInteger>>(getAmountsOutFunction, blockParameterVal)
            
        member this.QuoteQueryAsync(quoteFunction: QuoteFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<QuoteFunction, BigInteger>(quoteFunction, blockParameterVal)
            
        member this.RemoveLiquidityRequestAsync(removeLiquidityFunction: RemoveLiquidityFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityFunction);
        
        member this.RemoveLiquidityRequestAndWaitForReceiptAsync(removeLiquidityFunction: RemoveLiquidityFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityETHRequestAsync(removeLiquidityETHFunction: RemoveLiquidityETHFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityETHFunction);
        
        member this.RemoveLiquidityETHRequestAndWaitForReceiptAsync(removeLiquidityETHFunction: RemoveLiquidityETHFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction: RemoveLiquidityETHSupportingFeeOnTransferTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction);
        
        member this.RemoveLiquidityETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction: RemoveLiquidityETHSupportingFeeOnTransferTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHSupportingFeeOnTransferTokensFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityETHWithPermitRequestAsync(removeLiquidityETHWithPermitFunction: RemoveLiquidityETHWithPermitFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitFunction);
        
        member this.RemoveLiquidityETHWithPermitRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitFunction: RemoveLiquidityETHWithPermitFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction: RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction);
        
        member this.RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction: RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityWithPermitRequestAsync(removeLiquidityWithPermitFunction: RemoveLiquidityWithPermitFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityWithPermitFunction);
        
        member this.RemoveLiquidityWithPermitRequestAndWaitForReceiptAsync(removeLiquidityWithPermitFunction: RemoveLiquidityWithPermitFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityWithPermitFunction, cancellationTokenSourceVal);
        
        member this.SwapETHForExactTokensRequestAsync(swapETHForExactTokensFunction: SwapETHForExactTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapETHForExactTokensFunction);
        
        member this.SwapETHForExactTokensRequestAndWaitForReceiptAsync(swapETHForExactTokensFunction: SwapETHForExactTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapETHForExactTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapExactETHForTokensRequestAsync(swapExactETHForTokensFunction: SwapExactETHForTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactETHForTokensFunction);
        
        member this.SwapExactETHForTokensRequestAndWaitForReceiptAsync(swapExactETHForTokensFunction: SwapExactETHForTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactETHForTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction: SwapExactETHForTokensSupportingFeeOnTransferTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction);
        
        member this.SwapExactETHForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction: SwapExactETHForTokensSupportingFeeOnTransferTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactETHForTokensSupportingFeeOnTransferTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapExactTokensForETHRequestAsync(swapExactTokensForETHFunction: SwapExactTokensForETHFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactTokensForETHFunction);
        
        member this.SwapExactTokensForETHRequestAndWaitForReceiptAsync(swapExactTokensForETHFunction: SwapExactTokensForETHFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForETHFunction, cancellationTokenSourceVal);
        
        member this.SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction: SwapExactTokensForETHSupportingFeeOnTransferTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction);
        
        member this.SwapExactTokensForETHSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction: SwapExactTokensForETHSupportingFeeOnTransferTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForETHSupportingFeeOnTransferTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapExactTokensForTokensRequestAsync(swapExactTokensForTokensFunction: SwapExactTokensForTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactTokensForTokensFunction);
        
        member this.SwapExactTokensForTokensRequestAndWaitForReceiptAsync(swapExactTokensForTokensFunction: SwapExactTokensForTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction: SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction);
        
        member this.SwapExactTokensForTokensSupportingFeeOnTransferTokensRequestAndWaitForReceiptAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction: SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapExactTokensForTokensSupportingFeeOnTransferTokensFunction, cancellationTokenSourceVal);
        
        member this.SwapTokensForExactETHRequestAsync(swapTokensForExactETHFunction: SwapTokensForExactETHFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapTokensForExactETHFunction);
        
        member this.SwapTokensForExactETHRequestAndWaitForReceiptAsync(swapTokensForExactETHFunction: SwapTokensForExactETHFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactETHFunction, cancellationTokenSourceVal);
        
        member this.SwapTokensForExactTokensRequestAsync(swapTokensForExactTokensFunction: SwapTokensForExactTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(swapTokensForExactTokensFunction);
        
        member this.SwapTokensForExactTokensRequestAndWaitForReceiptAsync(swapTokensForExactTokensFunction: SwapTokensForExactTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(swapTokensForExactTokensFunction, cancellationTokenSourceVal);
        
    

