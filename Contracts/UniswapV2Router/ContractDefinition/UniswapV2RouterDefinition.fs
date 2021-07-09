namespace Contracts.UniswapV2Router.ContractDefinition

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

    
    
    type UniswapV2RouterDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = UniswapV2RouterDeployment(BYTECODE)
        
            [<Parameter("address", "_factory", 1)>]
            member val public Factory = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_WETH", 2)>]
            member val public WETH = Unchecked.defaultof<string> with get, set
        
    
    [<Function("WETH", "address")>]
    type WETHFunction() = 
        inherit FunctionMessage()
    
    [<FunctionOutput>]
    type RemoveLiquidityOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountA", 1)>]
            member val public AmountA = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountB", 2)>]
            member val public AmountB = Unchecked.defaultof<BigInteger> with get, set

    [<FunctionOutput>]
    type AddLiquidityOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountA", 1)>]
            member val public AmountA = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountB", 2)>]
            member val public AmountB = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "liquidity", 3)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set

    [<FunctionOutput>]
    type AddLiquidityETHOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountToken", 1)>]
            member val public AmountToken = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETH", 2)>]
            member val public AmountETH = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "liquidity", 3)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set

    [<FunctionOutput>]
    type RemoveLiquidityETHOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountToken", 1)>]
            member val public AmountToken = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETH", 2)>]
            member val public AmountETH = Unchecked.defaultof<BigInteger> with get, set

    [<FunctionOutput>]
    type RemoveLiquidityETHWithPermitOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountToken", 1)>]
            member val public AmountToken = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETH", 2)>]
            member val public AmountETH = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    [<FunctionOutput>]
    type RemoveLiquidityWithPermitOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountA", 1)>]
            member val public AmountA = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountB", 2)>]
            member val public AmountB = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("addLiquidity", typeof<AddLiquidityOutputDTO>)>]
    type AddLiquidityFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "tokenA", 1)>]
            member val public TokenA = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "tokenB", 2)>]
            member val public TokenB = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amountADesired", 3)>]
            member val public AmountADesired = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountBDesired", 4)>]
            member val public AmountBDesired = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountAMin", 5)>]
            member val public AmountAMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountBMin", 6)>]
            member val public AmountBMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 7)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 8)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("addLiquidityETH", typeof<AddLiquidityETHOutputDTO>)>]
    type AddLiquidityETHFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "amountTokenDesired", 2)>]
            member val public AmountTokenDesired = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountTokenMin", 3)>]
            member val public AmountTokenMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETHMin", 4)>]
            member val public AmountETHMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 5)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 6)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("factory", "address")>]
    type FactoryFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("getAmountIn", "uint256")>]
    type GetAmountInFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveIn", 2)>]
            member val public ReserveIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveOut", 3)>]
            member val public ReserveOut = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getAmountOut", "uint256")>]
    type GetAmountOutFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveIn", 2)>]
            member val public ReserveIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveOut", 3)>]
            member val public ReserveOut = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getAmountsIn", "uint256[]")>]
    type GetAmountsInFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 2)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
        
    
    [<Function("getAmountsOut", "uint256[]")>]
    type GetAmountsOutFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 2)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
        
    
    [<Function("quote", "uint256")>]
    type QuoteFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountA", 1)>]
            member val public AmountA = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveA", 2)>]
            member val public ReserveA = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "reserveB", 3)>]
            member val public ReserveB = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("removeLiquidity", typeof<RemoveLiquidityOutputDTO>)>]
    type RemoveLiquidityFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "tokenA", 1)>]
            member val public TokenA = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "tokenB", 2)>]
            member val public TokenB = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 3)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountAMin", 4)>]
            member val public AmountAMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountBMin", 5)>]
            member val public AmountBMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 6)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 7)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("removeLiquidityETH", typeof<RemoveLiquidityETHOutputDTO>)>]
    type RemoveLiquidityETHFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 2)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountTokenMin", 3)>]
            member val public AmountTokenMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETHMin", 4)>]
            member val public AmountETHMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 5)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 6)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("removeLiquidityETHSupportingFeeOnTransferTokens", "uint256")>]
    type RemoveLiquidityETHSupportingFeeOnTransferTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 2)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountTokenMin", 3)>]
            member val public AmountTokenMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETHMin", 4)>]
            member val public AmountETHMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 5)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 6)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("removeLiquidityETHWithPermit", typeof<RemoveLiquidityETHWithPermitOutputDTO>)>]
    type RemoveLiquidityETHWithPermitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 2)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountTokenMin", 3)>]
            member val public AmountTokenMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETHMin", 4)>]
            member val public AmountETHMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 5)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 6)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bool", "approveMax", 7)>]
            member val public ApproveMax = Unchecked.defaultof<bool> with get, set
            [<Parameter("uint8", "v", 8)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 9)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 10)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("removeLiquidityETHWithPermitSupportingFeeOnTransferTokens", "uint256")>]
    type RemoveLiquidityETHWithPermitSupportingFeeOnTransferTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token", 1)>]
            member val public Token = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 2)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountTokenMin", 3)>]
            member val public AmountTokenMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountETHMin", 4)>]
            member val public AmountETHMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 5)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 6)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bool", "approveMax", 7)>]
            member val public ApproveMax = Unchecked.defaultof<bool> with get, set
            [<Parameter("uint8", "v", 8)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 9)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 10)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("removeLiquidityWithPermit", typeof<RemoveLiquidityWithPermitOutputDTO>)>]
    type RemoveLiquidityWithPermitFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "tokenA", 1)>]
            member val public TokenA = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "tokenB", 2)>]
            member val public TokenB = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "liquidity", 3)>]
            member val public Liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountAMin", 4)>]
            member val public AmountAMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountBMin", 5)>]
            member val public AmountBMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "to", 6)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 7)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("bool", "approveMax", 8)>]
            member val public ApproveMax = Unchecked.defaultof<bool> with get, set
            [<Parameter("uint8", "v", 9)>]
            member val public V = Unchecked.defaultof<byte> with get, set
            [<Parameter("bytes32", "r", 10)>]
            member val public R = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("bytes32", "s", 11)>]
            member val public S = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("swapETHForExactTokens", "uint256[]")>]
    type SwapETHForExactTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 2)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 3)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactETHForTokens", "uint256[]")>]
    type SwapExactETHForTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOutMin", 1)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 2)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 3)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactETHForTokensSupportingFeeOnTransferTokens")>]
    type SwapExactETHForTokensSupportingFeeOnTransferTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOutMin", 1)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 2)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 3)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactTokensForETH", "uint256[]")>]
    type SwapExactTokensForETHFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMin", 2)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactTokensForETHSupportingFeeOnTransferTokens")>]
    type SwapExactTokensForETHSupportingFeeOnTransferTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMin", 2)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactTokensForTokens", "uint256[]")>]
    type SwapExactTokensForTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMin", 2)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapExactTokensForTokensSupportingFeeOnTransferTokens")>]
    type SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMin", 2)>]
            member val public AmountOutMin = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapTokensForExactETH", "uint256[]")>]
    type SwapTokensForExactETHFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountInMax", 2)>]
            member val public AmountInMax = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("swapTokensForExactTokens", "uint256[]")>]
    type SwapTokensForExactTokensFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountInMax", 2)>]
            member val public AmountInMax = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address[]", "path", 3)>]
            member val public Path = Unchecked.defaultof<List<string>> with get, set
            [<Parameter("address", "to", 4)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type WETHOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type FactoryOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type GetAmountInOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountIn", 1)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type GetAmountOutOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountOut", 1)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type GetAmountsInOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256[]", "amounts", 1)>]
            member val public Amounts = Unchecked.defaultof<List<BigInteger>> with get, set
        
    
    [<FunctionOutput>]
    type GetAmountsOutOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256[]", "amounts", 1)>]
            member val public Amounts = Unchecked.defaultof<List<BigInteger>> with get, set
        
    
    [<FunctionOutput>]
    type QuoteOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "amountB", 1)>]
            member val public AmountB = Unchecked.defaultof<BigInteger> with get, set
        
    
        
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    


