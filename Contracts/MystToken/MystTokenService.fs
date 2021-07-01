namespace Contracts.MystToken

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
open Contracts.MystToken.ContractDefinition


    type MystTokenService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, mystTokenDeployment: MystTokenDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<MystTokenDeployment>().SendRequestAndWaitForReceiptAsync(mystTokenDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, mystTokenDeployment: MystTokenDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<MystTokenDeployment>().SendRequestAsync(mystTokenDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, mystTokenDeployment: MystTokenDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = MystTokenService.DeployContractAndWaitForReceiptAsync(web3, mystTokenDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new MystTokenService(web3, receipt.ContractAddress);
            }
    
        member this.DOMAIN_SEPARATORQueryAsync(dOMAIN_SEPARATORFunction: DOMAIN_SEPARATORFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<DOMAIN_SEPARATORFunction, byte[]>(dOMAIN_SEPARATORFunction, blockParameterVal)
            
        member this.PERMIT_TYPEHASHQueryAsync(pERMIT_TYPEHASHFunction: PERMIT_TYPEHASHFunction, ?blockParameter: BlockParameter): Task<byte[]> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<PERMIT_TYPEHASHFunction, byte[]>(pERMIT_TYPEHASHFunction, blockParameterVal)
            
        member this.AllowanceQueryAsync(allowanceFunction: AllowanceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameterVal)
            
        member this.ApproveRequestAsync(approveFunction: ApproveFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(approveFunction);
        
        member this.ApproveRequestAndWaitForReceiptAsync(approveFunction: ApproveFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationTokenSourceVal);
        
        member this.BalanceOfQueryAsync(balanceOfFunction: BalanceOfFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameterVal)
            
        member this.BurnRequestAsync(burnFunction: BurnFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(burnFunction);
        
        member this.BurnRequestAndWaitForReceiptAsync(burnFunction: BurnFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(burnFunction, cancellationTokenSourceVal);
        
        member this.ClaimTokensRequestAsync(claimTokensFunction: ClaimTokensFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(claimTokensFunction);
        
        member this.ClaimTokensRequestAndWaitForReceiptAsync(claimTokensFunction: ClaimTokensFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(claimTokensFunction, cancellationTokenSourceVal);
        
        member this.DecimalsQueryAsync(decimalsFunction: DecimalsFunction, ?blockParameter: BlockParameter): Task<byte> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<DecimalsFunction, byte>(decimalsFunction, blockParameterVal)
            
        member this.DecreaseAllowanceRequestAsync(decreaseAllowanceFunction: DecreaseAllowanceFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        
        member this.DecreaseAllowanceRequestAndWaitForReceiptAsync(decreaseAllowanceFunction: DecreaseAllowanceFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationTokenSourceVal);
        
        member this.GetFundsDestinationQueryAsync(getFundsDestinationFunction: GetFundsDestinationFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetFundsDestinationFunction, string>(getFundsDestinationFunction, blockParameterVal)
            
        member this.GetUpgradeStateQueryAsync(getUpgradeStateFunction: GetUpgradeStateFunction, ?blockParameter: BlockParameter): Task<byte> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<GetUpgradeStateFunction, byte>(getUpgradeStateFunction, blockParameterVal)
            
        member this.IncreaseAllowanceRequestAsync(increaseAllowanceFunction: IncreaseAllowanceFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        
        member this.IncreaseAllowanceRequestAndWaitForReceiptAsync(increaseAllowanceFunction: IncreaseAllowanceFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationTokenSourceVal);
        
        member this.IsUpgradeAgentQueryAsync(isUpgradeAgentFunction: IsUpgradeAgentFunction, ?blockParameter: BlockParameter): Task<bool> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<IsUpgradeAgentFunction, bool>(isUpgradeAgentFunction, blockParameterVal)
            
        member this.NameQueryAsync(nameFunction: NameFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameterVal)
            
        member this.NoncesQueryAsync(noncesFunction: NoncesFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<NoncesFunction, BigInteger>(noncesFunction, blockParameterVal)
            
        member this.OriginalSupplyQueryAsync(originalSupplyFunction: OriginalSupplyFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<OriginalSupplyFunction, BigInteger>(originalSupplyFunction, blockParameterVal)
            
        member this.OriginalTokenQueryAsync(originalTokenFunction: OriginalTokenFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<OriginalTokenFunction, string>(originalTokenFunction, blockParameterVal)
            
        member this.PermitRequestAsync(permitFunction: PermitFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(permitFunction);
        
        member this.PermitRequestAndWaitForReceiptAsync(permitFunction: PermitFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(permitFunction, cancellationTokenSourceVal);
        
        member this.SetFundsDestinationRequestAsync(setFundsDestinationFunction: SetFundsDestinationFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(setFundsDestinationFunction);
        
        member this.SetFundsDestinationRequestAndWaitForReceiptAsync(setFundsDestinationFunction: SetFundsDestinationFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(setFundsDestinationFunction, cancellationTokenSourceVal);
        
        member this.SetUpgradeAgentRequestAsync(setUpgradeAgentFunction: SetUpgradeAgentFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(setUpgradeAgentFunction);
        
        member this.SetUpgradeAgentRequestAndWaitForReceiptAsync(setUpgradeAgentFunction: SetUpgradeAgentFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(setUpgradeAgentFunction, cancellationTokenSourceVal);
        
        member this.SetUpgradeMasterRequestAsync(setUpgradeMasterFunction: SetUpgradeMasterFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(setUpgradeMasterFunction);
        
        member this.SetUpgradeMasterRequestAndWaitForReceiptAsync(setUpgradeMasterFunction: SetUpgradeMasterFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(setUpgradeMasterFunction, cancellationTokenSourceVal);
        
        member this.SymbolQueryAsync(symbolFunction: SymbolFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<SymbolFunction, string>(symbolFunction, blockParameterVal)
            
        member this.TotalSupplyQueryAsync(totalSupplyFunction: TotalSupplyFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameterVal)
            
        member this.TotalUpgradedQueryAsync(totalUpgradedFunction: TotalUpgradedFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<TotalUpgradedFunction, BigInteger>(totalUpgradedFunction, blockParameterVal)
            
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
        
        member this.UpgradeRequestAsync(upgradeFunction: UpgradeFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(upgradeFunction);
        
        member this.UpgradeRequestAndWaitForReceiptAsync(upgradeFunction: UpgradeFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(upgradeFunction, cancellationTokenSourceVal);
        
        member this.UpgradeAgentQueryAsync(upgradeAgentFunction: UpgradeAgentFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<UpgradeAgentFunction, string>(upgradeAgentFunction, blockParameterVal)
            
        member this.UpgradeFromRequestAsync(upgradeFromFunction: UpgradeFromFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(upgradeFromFunction);
        
        member this.UpgradeFromRequestAndWaitForReceiptAsync(upgradeFromFunction: UpgradeFromFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(upgradeFromFunction, cancellationTokenSourceVal);
        
        member this.UpgradeMasterQueryAsync(upgradeMasterFunction: UpgradeMasterFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<UpgradeMasterFunction, string>(upgradeMasterFunction, blockParameterVal)
            
    

