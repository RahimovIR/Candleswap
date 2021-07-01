namespace Contracts.Path.ContractDefinition

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

    
    
    type PathDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = PathDeployment(BYTECODE)
        

     [<FunctionOutput>]
     type DecodeFirstPoolOutputDTO() =
         inherit FunctionOutputDTO() 
             [<Parameter("address", "tokenA", 1)>]
             member val public TokenA = Unchecked.defaultof<string> with get, set
             [<Parameter("address", "tokenB", 2)>]
             member val public TokenB = Unchecked.defaultof<string> with get, set
             [<Parameter("uint24", "fee", 3)>]
             member val public Fee = Unchecked.defaultof<BigInteger> with get, set   
    
    [<Function("decodeFirstPool", typeof<DecodeFirstPoolOutputDTO>)>]
    type DecodeFirstPoolFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("getFirstPool", "bytes")>]
    type GetFirstPoolFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("hasMultiplePools", "bool")>]
    type HasMultiplePoolsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("numPools", "uint256")>]
    type NumPoolsFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
        
    
    [<Function("skipToken", "bytes")>]
    type SkipTokenFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("bytes", "path", 1)>]
            member val public Path = Unchecked.defaultof<byte[]> with get, set
        
        
    
    [<FunctionOutput>]
    type GetFirstPoolOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type HasMultiplePoolsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bool", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<bool> with get, set
        
    
    [<FunctionOutput>]
    type NumPoolsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type SkipTokenOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes", "", 1)>]
            member val public ReturnValue1 = Unchecked.defaultof<byte[]> with get, set
    

