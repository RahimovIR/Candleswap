namespace Contracts.Router.ContractDefinition

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

    
    
    type RouterDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = RouterDeployment(BYTECODE)
        
            [<Parameter("address", "_factory", 1)>]
            member val public Factory = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_WETH9", 2)>]
            member val public WETH9 = Unchecked.defaultof<string> with get, set
        
    
    [<Function("WETH9", "address")>]
    type WETH9Function() = 
        inherit FunctionMessage()
    

        
    
    [<Function("exactInput", "uint256")>]
    type ExactInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("tuple", "params", 1)>]
            member val public Params = Unchecked.defaultof<ExactInputParams> with get, set
        
    
    [<Function("exactInputSingle", "uint256")>]
    type ExactInputSingleFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("tuple", "params", 1)>]
            member val public Params = Unchecked.defaultof<ExactInputSingleParams> with get, set
        
    
    [<Function("exactOutput", "uint256")>]
    type ExactOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("tuple", "params", 1)>]
            member val public Params = Unchecked.defaultof<ExactOutputParams> with get, set
        
    
    [<Function("exactOutputSingle", "uint256")>]
    type ExactOutputSingleFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("tuple", "params", 1)>]
            member val public Params = Unchecked.defaultof<ExactOutputSingleParams> with get, set
        
    
    [<Function("factory", "address")>]
    type FactoryFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("multicall", "bytes[]")>]
    type MulticallFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes[]", "data", 1)>]
            member val public Data = Unchecked.defaultof<List<byte[]>> with get, set
        
    
    [<Function("refundETH")>]
    type RefundETHFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("selfPermit")>]
    type SelfPermitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint8", "v", 4)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 5)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 6)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("selfPermitAllowed")>]
    type SelfPermitAllowedFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "nonce", 2)>]
            member val public Nonce = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "expiry", 3)>]
            member val public Expiry = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint8", "v", 4)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 5)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 6)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("selfPermitAllowedIfNecessary")>]
    type SelfPermitAllowedIfNecessaryFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "nonce", 2)>]
            member val public Nonce = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "expiry", 3)>]
            member val public Expiry = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint8", "v", 4)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 5)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 6)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("selfPermitIfNecessary")>]
    type SelfPermitIfNecessaryFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint8", "v", 4)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 5)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 6)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("sweepToken")>]
    type SweepTokenFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amountMinimum", 2)>]
            member val public AmountMinimum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 3)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("sweepTokenWithFee")>]
    type SweepTokenWithFeeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amountMinimum", 2)>]
            member val public AmountMinimum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 3)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "feeBips", 4)>]
            member val public FeeBips = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "feeRecipient", 5)>]
            member val public FeeRecipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("uniswapV3SwapCallback")>]
    type UniswapV3SwapCallbackFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("int256", "amount0Delta", 1)>]
            member val public Amount0Delta = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("int256", "amount1Delta", 2)>]
            member val public Amount1Delta = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bytes", "_data", 3)>]
            member val public Data = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("unwrapWETH9")>]
    type UnwrapWETH9Function() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountMinimum", 1)>]
            member val public AmountMinimum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 2)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("unwrapWETH9WithFee")>]
    type UnwrapWETH9WithFeeFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountMinimum", 1)>]
            member val public AmountMinimum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 2)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "feeBips", 3)>]
            member val public FeeBips = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "feeRecipient", 4)>]
            member val public FeeRecipient = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type WETH9OutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type FactoryOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    


