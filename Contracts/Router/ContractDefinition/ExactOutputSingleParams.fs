namespace Contracts.Router.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes

    type ExactOutputSingleParams() =
            [<Parameter("address", "tokenIn", 1)>]
            member val public TokenIn = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "tokenOut", 2)>]
            member val public TokenOut = Unchecked.defaultof<string> with get, set
            [<Parameter("uint24", "fee", 3)>]
            member val public Fee = Unchecked.defaultof<bigint> with get, set //Change here
            [<Parameter("address", "recipient", 4)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 5)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOut", 6)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountInMaximum", 7)>]
            member val public AmountInMaximum = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint160", "sqrtPriceLimitX96", 8)>]
            member val public SqrtPriceLimitX96 = Unchecked.defaultof<BigInteger> with get, set
    

