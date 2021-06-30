namespace Contracts.Router.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes

    type ExactInputSingleParams() =
            [<Parameter("address", "tokenIn", 1)>]
            member val public TokenIn = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "tokenOut", 2)>]
            member val public TokenOut = Unchecked.defaultof<string> with get, set
            [<Parameter("uint24", "fee", 3)>]
            member val public Fee = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 4)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountIn", 6)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMinimum", 7)>]
            member val public AmountOutMinimum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint160", "sqrtPriceLimitX96", 8)>]
            member val public SqrtPriceLimitX96 = Unchecked.defaultof<BigInteger> with get, set
    

