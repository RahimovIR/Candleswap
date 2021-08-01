namespace Contracts.UniswapV1Exchange

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
open Contracts.UniswapV1Exchange.ContractDefinition


    type UniswapV1ExchangeService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, uniswapV1ExchangeDeployment: UniswapV1ExchangeDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<UniswapV1ExchangeDeployment>().SendRequestAndWaitForReceiptAsync(uniswapV1ExchangeDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, uniswapV1ExchangeDeployment: UniswapV1ExchangeDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<UniswapV1ExchangeDeployment>().SendRequestAsync(uniswapV1ExchangeDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, uniswapV1ExchangeDeployment: UniswapV1ExchangeDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = UniswapV1ExchangeService.DeployContractAndWaitForReceiptAsync(web3, uniswapV1ExchangeDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new UniswapV1ExchangeService(web3, receipt.ContractAddress);
            }
    
        member this.SetupRequestAsync(setupFunction: SetupFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(setupFunction);
        
        member this.SetupRequestAndWaitForReceiptAsync(setupFunction: SetupFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(setupFunction, cancellationTokenSourceVal);
        
        member this.AddLiquidityRequestAsync(addLiquidityFunction: AddLiquidityFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(addLiquidityFunction);
        
        member this.AddLiquidityRequestAndWaitForReceiptAsync(addLiquidityFunction: AddLiquidityFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(addLiquidityFunction, cancellationTokenSourceVal);
        
        member this.RemoveLiquidityRequestAsync(removeLiquidityFunction: RemoveLiquidityFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(removeLiquidityFunction);
        
        member this.RemoveLiquidityRequestAndWaitForReceiptAsync(removeLiquidityFunction: RemoveLiquidityFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(removeLiquidityFunction, cancellationTokenSourceVal);
        
        member this.Default__RequestAsync(default__Function: Default__Function): Task<string> =
            this.ContractHandler.SendRequestAsync(default__Function);
        
        member this.Default__RequestAndWaitForReceiptAsync(default__Function: Default__Function, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(default__Function, cancellationTokenSourceVal);
        
        member this.EthToTokenSwapInputRequestAsync(ethToTokenSwapInputFunction: EthToTokenSwapInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(ethToTokenSwapInputFunction);
        
        member this.EthToTokenSwapInputRequestAndWaitForReceiptAsync(ethToTokenSwapInputFunction: EthToTokenSwapInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(ethToTokenSwapInputFunction, cancellationTokenSourceVal);
        
        member this.EthToTokenTransferInputRequestAsync(ethToTokenTransferInputFunction: EthToTokenTransferInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(ethToTokenTransferInputFunction);
        
        member this.EthToTokenTransferInputRequestAndWaitForReceiptAsync(ethToTokenTransferInputFunction: EthToTokenTransferInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(ethToTokenTransferInputFunction, cancellationTokenSourceVal);
        
        member this.EthToTokenSwapOutputRequestAsync(ethToTokenSwapOutputFunction: EthToTokenSwapOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(ethToTokenSwapOutputFunction);
        
        member this.EthToTokenSwapOutputRequestAndWaitForReceiptAsync(ethToTokenSwapOutputFunction: EthToTokenSwapOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(ethToTokenSwapOutputFunction, cancellationTokenSourceVal);
        
        member this.EthToTokenTransferOutputRequestAsync(ethToTokenTransferOutputFunction: EthToTokenTransferOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(ethToTokenTransferOutputFunction);
        
        member this.EthToTokenTransferOutputRequestAndWaitForReceiptAsync(ethToTokenTransferOutputFunction: EthToTokenTransferOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(ethToTokenTransferOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToEthSwapInputRequestAsync(tokenToEthSwapInputFunction: TokenToEthSwapInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToEthSwapInputFunction);
        
        member this.TokenToEthSwapInputRequestAndWaitForReceiptAsync(tokenToEthSwapInputFunction: TokenToEthSwapInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToEthSwapInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToEthTransferInputRequestAsync(tokenToEthTransferInputFunction: TokenToEthTransferInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToEthTransferInputFunction);
        
        member this.TokenToEthTransferInputRequestAndWaitForReceiptAsync(tokenToEthTransferInputFunction: TokenToEthTransferInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToEthTransferInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToEthSwapOutputRequestAsync(tokenToEthSwapOutputFunction: TokenToEthSwapOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToEthSwapOutputFunction);
        
        member this.TokenToEthSwapOutputRequestAndWaitForReceiptAsync(tokenToEthSwapOutputFunction: TokenToEthSwapOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToEthSwapOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToEthTransferOutputRequestAsync(tokenToEthTransferOutputFunction: TokenToEthTransferOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToEthTransferOutputFunction);
        
        member this.TokenToEthTransferOutputRequestAndWaitForReceiptAsync(tokenToEthTransferOutputFunction: TokenToEthTransferOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToEthTransferOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToTokenSwapInputRequestAsync(tokenToTokenSwapInputFunction: TokenToTokenSwapInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToTokenSwapInputFunction);
        
        member this.TokenToTokenSwapInputRequestAndWaitForReceiptAsync(tokenToTokenSwapInputFunction: TokenToTokenSwapInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToTokenSwapInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToTokenTransferInputRequestAsync(tokenToTokenTransferInputFunction: TokenToTokenTransferInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToTokenTransferInputFunction);
        
        member this.TokenToTokenTransferInputRequestAndWaitForReceiptAsync(tokenToTokenTransferInputFunction: TokenToTokenTransferInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToTokenTransferInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToTokenSwapOutputRequestAsync(tokenToTokenSwapOutputFunction: TokenToTokenSwapOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToTokenSwapOutputFunction);
        
        member this.TokenToTokenSwapOutputRequestAndWaitForReceiptAsync(tokenToTokenSwapOutputFunction: TokenToTokenSwapOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToTokenSwapOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToTokenTransferOutputRequestAsync(tokenToTokenTransferOutputFunction: TokenToTokenTransferOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToTokenTransferOutputFunction);
        
        member this.TokenToTokenTransferOutputRequestAndWaitForReceiptAsync(tokenToTokenTransferOutputFunction: TokenToTokenTransferOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToTokenTransferOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToExchangeSwapInputRequestAsync(tokenToExchangeSwapInputFunction: TokenToExchangeSwapInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToExchangeSwapInputFunction);
        
        member this.TokenToExchangeSwapInputRequestAndWaitForReceiptAsync(tokenToExchangeSwapInputFunction: TokenToExchangeSwapInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToExchangeSwapInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToExchangeTransferInputRequestAsync(tokenToExchangeTransferInputFunction: TokenToExchangeTransferInputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToExchangeTransferInputFunction);
        
        member this.TokenToExchangeTransferInputRequestAndWaitForReceiptAsync(tokenToExchangeTransferInputFunction: TokenToExchangeTransferInputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToExchangeTransferInputFunction, cancellationTokenSourceVal);
        
        member this.TokenToExchangeSwapOutputRequestAsync(tokenToExchangeSwapOutputFunction: TokenToExchangeSwapOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToExchangeSwapOutputFunction);
        
        member this.TokenToExchangeSwapOutputRequestAndWaitForReceiptAsync(tokenToExchangeSwapOutputFunction: TokenToExchangeSwapOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToExchangeSwapOutputFunction, cancellationTokenSourceVal);
        
        member this.TokenToExchangeTransferOutputRequestAsync(tokenToExchangeTransferOutputFunction: TokenToExchangeTransferOutputFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(tokenToExchangeTransferOutputFunction);
        
        member this.TokenToExchangeTransferOutputRequestAndWaitForReceiptAsync(tokenToExchangeTransferOutputFunction: TokenToExchangeTransferOutputFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(tokenToExchangeTransferOutputFunction, cancellationTokenSourceVal);
        
        member this.GetEthToTokenInputPriceQueryAsync(getEthToTokenInputPriceFunction: GetEthToTokenInputPriceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetEthToTokenInputPriceFunction, BigInteger>(getEthToTokenInputPriceFunction, blockParameterVal)
            
        member this.GetEthToTokenOutputPriceQueryAsync(getEthToTokenOutputPriceFunction: GetEthToTokenOutputPriceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetEthToTokenOutputPriceFunction, BigInteger>(getEthToTokenOutputPriceFunction, blockParameterVal)
            
        member this.GetTokenToEthInputPriceQueryAsync(getTokenToEthInputPriceFunction: GetTokenToEthInputPriceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetTokenToEthInputPriceFunction, BigInteger>(getTokenToEthInputPriceFunction, blockParameterVal)
            
        member this.GetTokenToEthOutputPriceQueryAsync(getTokenToEthOutputPriceFunction: GetTokenToEthOutputPriceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetTokenToEthOutputPriceFunction, BigInteger>(getTokenToEthOutputPriceFunction, blockParameterVal)
            
        member this.TokenAddressQueryAsync(tokenAddressFunction: TokenAddressFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<TokenAddressFunction, string>(tokenAddressFunction, blockParameterVal)
            
        member this.FactoryAddressQueryAsync(factoryAddressFunction: FactoryAddressFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<FactoryAddressFunction, string>(factoryAddressFunction, blockParameterVal)
            
        member this.BalanceOfQueryAsync(balanceOfFunction: BalanceOfFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameterVal)
            
        member this.TransferRequestAsync(transferFunction: TransferFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(transferFunction);
        
        member this.TransferRequestAndWaitForReceiptAsync(transferFunction: TransferFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationTokenSourceVal);
        
        member this.TransferFromRequestAsync(transferFromFunction: TransferFromFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(transferFromFunction);
        
        member this.TransferFromRequestAndWaitForReceiptAsync(transferFromFunction: TransferFromFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationTokenSourceVal);
        
        member this.ApproveRequestAsync(approveFunction: ApproveFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(approveFunction);
        
        member this.ApproveRequestAndWaitForReceiptAsync(approveFunction: ApproveFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationTokenSourceVal);
        
        member this.AllowanceQueryAsync(allowanceFunction: AllowanceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameterVal)
            
        member this.NameQueryAsync(nameFunction: NameFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<NameFunction, byte[]>(nameFunction, blockParameterVal)
            
        member this.SymbolQueryAsync(symbolFunction: SymbolFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<SymbolFunction, byte[]>(symbolFunction, blockParameterVal)
            
        member this.DecimalsQueryAsync(decimalsFunction: DecimalsFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<DecimalsFunction, BigInteger>(decimalsFunction, blockParameterVal)
            
        member this.TotalSupplyQueryAsync(totalSupplyFunction: TotalSupplyFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameterVal)
            
    

