namespace Contracts.Router.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes

    type ExactInputParams() =
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "recipient", 2)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountIn", 4)>]
            member val public AmountIn = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOutMinimum", 5)>]
            member val public AmountOutMinimum = Unchecked.defaultof<BigInteger> with get, set
    

