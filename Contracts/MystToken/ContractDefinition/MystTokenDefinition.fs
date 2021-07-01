namespace Contracts.MystToken.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts
open System.Threading

    
    
    type MystTokenDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = MystTokenDeployment(BYTECODE)
        
            [<Parameter("address", "originalToken", 1)>]
            member val public OriginalToken = Unchecked.defaultof<string> with get, set
        
    
    [<Function("DOMAIN_SEPARATOR", "bytes32")>]
    type DOMAIN_SEPARATORFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("PERMIT_TYPEHASH", "bytes32")>]
    type PERMIT_TYPEHASHFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("allowance", "uint256")>]
    type AllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "holder", 1)>]
            member val public Holder = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "spender", 2)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
        
    
    [<Function("approve", "bool")>]
    type ApproveFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("balanceOf", "uint256")>]
    type BalanceOfFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "tokenHolder", 1)>]
            member val public TokenHolder = Unchecked.defaultof<string> with get, set
        
    
    [<Function("burn")>]
    type BurnFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amount", 1)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("claimTokens")>]
    type ClaimTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
        
    
    [<Function("decimals", "uint8")>]
    type DecimalsFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("decreaseAllowance", "bool")>]
    type DecreaseAllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "subtractedValue", 2)>]
            member val public SubtractedValue = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getFundsDestination", "address")>]
    type GetFundsDestinationFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("getUpgradeState", "uint8")>]
    type GetUpgradeStateFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("increaseAllowance", "bool")>]
    type IncreaseAllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "addedValue", 2)>]
            member val public AddedValue = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("isUpgradeAgent", "bool")>]
    type IsUpgradeAgentFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("name", "string")>]
    type NameFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("nonces", "uint256")>]
    type NoncesFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("originalSupply", "uint256")>]
    type OriginalSupplyFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("originalToken", "address")>]
    type OriginalTokenFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("permit")>]
    type PermitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "holder", 1)>]
            member val public Holder = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "spender", 2)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 3)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint8", "v", 5)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 6)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 7)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("setFundsDestination")>]
    type SetFundsDestinationFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "newDestination", 1)>]
            member val public NewDestination = Unchecked.defaultof<string> with get, set
        
    
    [<Function("setUpgradeAgent")>]
    type SetUpgradeAgentFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "agent", 1)>]
            member val public Agent = Unchecked.defaultof<string> with get, set
        
    
    [<Function("setUpgradeMaster")>]
    type SetUpgradeMasterFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "newUpgradeMaster", 1)>]
            member val public NewUpgradeMaster = Unchecked.defaultof<string> with get, set
        
    
    [<Function("symbol", "string")>]
    type SymbolFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("totalSupply", "uint256")>]
    type TotalSupplyFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("totalUpgraded", "uint256")>]
    type TotalUpgradedFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("transfer", "bool")>]
    type TransferFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "recipient", 1)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 2)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("transferFrom", "bool")>]
    type TransferFromFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "holder", 1)>]
            member val public Holder = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "recipient", 2)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 3)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("upgrade")>]
    type UpgradeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amount", 1)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("upgradeAgent", "address")>]
    type UpgradeAgentFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("upgradeFrom")>]
    type UpgradeFromFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_account", 1)>]
            member val public Account = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("upgradeMaster", "address")>]
    type UpgradeMasterFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Event("Approval")>]
    type ApprovalEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "owner", 1, true )>]
            member val Owner = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "spender", 2, true )>]
            member val Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Burned")>]
    type BurnedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "from", 1, true )>]
            member val From = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 2, false )>]
            member val Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("FundsRecoveryDestinationChanged")>]
    type FundsRecoveryDestinationChangedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "previousDestination", 1, true )>]
            member val PreviousDestination = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "newDestination", 2, true )>]
            member val NewDestination = Unchecked.defaultof<string> with get, set
        
    
    [<Event("Minted")>]
    type MintedEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "to", 1, true )>]
            member val To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amount", 2, false )>]
            member val Amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Transfer")>]
    type TransferEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "from", 1, true )>]
            member val From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "to", 2, true )>]
            member val To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Upgrade")>]
    type UpgradeEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "from", 1, true )>]
            member val From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "agent", 2, false )>]
            member val Agent = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("UpgradeAgentSet")>]
    type UpgradeAgentSetEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "agent", 1, false )>]
            member val Agent = Unchecked.defaultof<string> with get, set
        
    
    [<Event("UpgradeMasterSet")>]
    type UpgradeMasterSetEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "master", 1, false )>]
            member val Master = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type DOMAIN_SEPARATOROutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type PERMIT_TYPEHASHOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type AllowanceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    [<FunctionOutput>]
    type BalanceOfOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    [<FunctionOutput>]
    type DecimalsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint8", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte> with get, set
        
    
    
    
    [<FunctionOutput>]
    type GetFundsDestinationOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type GetUpgradeStateOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint8", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte> with get, set
        
    
    
    
    [<FunctionOutput>]
    type IsUpgradeAgentOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bool", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<bool> with get, set
        
    
    [<FunctionOutput>]
    type NameOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("string", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type NoncesOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type OriginalSupplyOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type OriginalTokenOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type SymbolOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("string", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type TotalSupplyOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type TotalUpgradedOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type UpgradeAgentOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    
    
    [<FunctionOutput>]
    type UpgradeMasterOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
    

