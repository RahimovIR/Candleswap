namespace Contracts.Router.ContractDefinition

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes

    type ExactOutputParams() =
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
            [<Parameter("address", "recipient", 2)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountOut", 4)>]
            member val public AmountOut = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "amountInMaximum", 5)>]
            member val public AmountInMaximum = Unchecked.defaultof<BigInteger> with get, set
    

