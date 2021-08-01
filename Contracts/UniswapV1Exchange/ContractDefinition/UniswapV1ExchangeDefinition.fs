namespace Contracts.UniswapV1Exchange.ContractDefinition

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

    
    
    type UniswapV1ExchangeDeployment(byteCode: string) =
        inherit ContractDeploymentMessage(byteCode)
        
        static let BYTECODE = ""
        
        new() = UniswapV1ExchangeDeployment(BYTECODE)
        

        
    
    [<Function("setup")>]
    type SetupFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "token_addr", 1)>]
            member val public Token_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("addLiquidity", "uint256")>]
    type AddLiquidityFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "min_liquidity", 1)>]
            member val public Min_liquidity = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens", 2)>]
            member val public Max_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set

    [<FunctionOutput>]
    type RemoveLiquidityOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
            (*[<Parameter("uint256", "out", 2)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set*)
        
    
    [<Function("removeLiquidity", typeof<RemoveLiquidityOutputDTO>)>]
    type RemoveLiquidityFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "amount", 1)>]
            member val public Amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth", 2)>]
            member val public Min_eth = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_tokens", 3)>]
            member val public Min_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("__default__")>]
    type Default__Function() = 
        inherit FunctionMessage()
    

        
    
    [<Function("ethToTokenSwapInput", "uint256")>]
    type EthToTokenSwapInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "min_tokens", 1)>]
            member val public Min_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 2)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("ethToTokenTransferInput", "uint256")>]
    type EthToTokenTransferInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "min_tokens", 1)>]
            member val public Min_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 2)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 3)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("ethToTokenSwapOutput", "uint256")>]
    type EthToTokenSwapOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 2)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("ethToTokenTransferOutput", "uint256")>]
    type EthToTokenTransferOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 2)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 3)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToEthSwapInput", "uint256")>]
    type TokenToEthSwapInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth", 2)>]
            member val public Min_eth = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("tokenToEthTransferInput", "uint256")>]
    type TokenToEthTransferInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth", 2)>]
            member val public Min_eth = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 4)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToEthSwapOutput", "uint256")>]
    type TokenToEthSwapOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "eth_bought", 1)>]
            member val public Eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens", 2)>]
            member val public Max_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("tokenToEthTransferOutput", "uint256")>]
    type TokenToEthTransferOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "eth_bought", 1)>]
            member val public Eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens", 2)>]
            member val public Max_tokens = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 3)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 4)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToTokenSwapInput", "uint256")>]
    type TokenToTokenSwapInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_tokens_bought", 2)>]
            member val public Min_tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth_bought", 3)>]
            member val public Min_eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "token_addr", 5)>]
            member val public Token_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToTokenTransferInput", "uint256")>]
    type TokenToTokenTransferInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_tokens_bought", 2)>]
            member val public Min_tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth_bought", 3)>]
            member val public Min_eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 5)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "token_addr", 6)>]
            member val public Token_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToTokenSwapOutput", "uint256")>]
    type TokenToTokenSwapOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens_sold", 2)>]
            member val public Max_tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_eth_sold", 3)>]
            member val public Max_eth_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "token_addr", 5)>]
            member val public Token_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToTokenTransferOutput", "uint256")>]
    type TokenToTokenTransferOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens_sold", 2)>]
            member val public Max_tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_eth_sold", 3)>]
            member val public Max_eth_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 5)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "token_addr", 6)>]
            member val public Token_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToExchangeSwapInput", "uint256")>]
    type TokenToExchangeSwapInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_tokens_bought", 2)>]
            member val public Min_tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth_bought", 3)>]
            member val public Min_eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "exchange_addr", 5)>]
            member val public Exchange_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToExchangeTransferInput", "uint256")>]
    type TokenToExchangeTransferInputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_tokens_bought", 2)>]
            member val public Min_tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "min_eth_bought", 3)>]
            member val public Min_eth_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 5)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "exchange_addr", 6)>]
            member val public Exchange_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToExchangeSwapOutput", "uint256")>]
    type TokenToExchangeSwapOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens_sold", 2)>]
            member val public Max_tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_eth_sold", 3)>]
            member val public Max_eth_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "exchange_addr", 5)>]
            member val public Exchange_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("tokenToExchangeTransferOutput", "uint256")>]
    type TokenToExchangeTransferOutputFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_tokens_sold", 2)>]
            member val public Max_tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "max_eth_sold", 3)>]
            member val public Max_eth_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "deadline", 4)>]
            member val public Deadline = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("address", "recipient", 5)>]
            member val public Recipient = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "exchange_addr", 6)>]
            member val public Exchange_addr = Unchecked.defaultof<string> with get, set
        
    
    [<Function("getEthToTokenInputPrice", "uint256")>]
    type GetEthToTokenInputPriceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "eth_sold", 1)>]
            member val public Eth_sold = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getEthToTokenOutputPrice", "uint256")>]
    type GetEthToTokenOutputPriceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_bought", 1)>]
            member val public Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getTokenToEthInputPrice", "uint256")>]
    type GetTokenToEthInputPriceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "tokens_sold", 1)>]
            member val public Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("getTokenToEthOutputPrice", "uint256")>]
    type GetTokenToEthOutputPriceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("uint256", "eth_bought", 1)>]
            member val public Eth_bought = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("tokenAddress", "address")>]
    type TokenAddressFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("factoryAddress", "address")>]
    type FactoryAddressFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("balanceOf", "uint256")>]
    type BalanceOfFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_owner", 1)>]
            member val public Owner = Unchecked.defaultof<string> with get, set
        
    
    [<Function("transfer", "bool")>]
    type TransferFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_to", 1)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("transferFrom", "bool")>]
    type TransferFromFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_from", 1)>]
            member val public From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_to", 2)>]
            member val public To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 3)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("approve", "bool")>]
    type ApproveFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_spender", 1)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 2)>]
            member val public Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Function("allowance", "uint256")>]
    type AllowanceFunction() = 
        inherit FunctionMessage()
    
            [<Parameter("address", "_owner", 1)>]
            member val public Owner = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_spender", 2)>]
            member val public Spender = Unchecked.defaultof<string> with get, set
        
    
    [<Function("name", "bytes32")>]
    type NameFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("symbol", "bytes32")>]
    type SymbolFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("decimals", "uint256")>]
    type DecimalsFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Function("totalSupply", "uint256")>]
    type TotalSupplyFunction() = 
        inherit FunctionMessage()
    

        
    
    [<Event("TokenPurchase")>]
    type TokenPurchaseEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "buyer", 1, true )>]
            member val Buyer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "eth_sold", 2, true )>]
            member val Eth_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "tokens_bought", 3, true )>]
            member val Tokens_bought = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("EthPurchase")>]
    type EthPurchaseEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "buyer", 1, true )>]
            member val Buyer = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "tokens_sold", 2, true )>]
            member val Tokens_sold = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "eth_bought", 3, true )>]
            member val Eth_bought = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("AddLiquidity")>]
    type AddLiquidityEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "provider", 1, true )>]
            member val Provider = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "eth_amount", 2, true )>]
            member val Eth_amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "token_amount", 3, true )>]
            member val Token_amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("RemoveLiquidity")>]
    type RemoveLiquidityEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "provider", 1, true )>]
            member val Provider = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "eth_amount", 2, true )>]
            member val Eth_amount = Unchecked.defaultof<BigInteger> with get, set
            [<Parameter("uint256", "token_amount", 3, true )>]
            member val Token_amount = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Transfer")>]
    type TransferEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_from", 1, true )>]
            member val From = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_to", 2, true )>]
            member val To = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<Event("Approval")>]
    type ApprovalEventDTO() =
        inherit EventDTO()
            [<Parameter("address", "_owner", 1, true )>]
            member val Owner = Unchecked.defaultof<string> with get, set
            [<Parameter("address", "_spender", 2, true )>]
            member val Spender = Unchecked.defaultof<string> with get, set
            [<Parameter("uint256", "_value", 3, false )>]
            member val Value = Unchecked.defaultof<BigInteger> with get, set
        
    
    
        
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type GetEthToTokenInputPriceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type GetEthToTokenOutputPriceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type GetTokenToEthInputPriceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type GetTokenToEthOutputPriceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type TokenAddressOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "out", 1)>]
            member val public Out = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type FactoryAddressOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("address", "out", 1)>]
            member val public Out = Unchecked.defaultof<string> with get, set
        
    
    [<FunctionOutput>]
    type BalanceOfOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    
    
    
    
    
    
    [<FunctionOutput>]
    type AllowanceOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type NameOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "out", 1)>]
            member val public Out = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type SymbolOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("bytes32", "out", 1)>]
            member val public Out = Unchecked.defaultof<byte[]> with get, set
        
    
    [<FunctionOutput>]
    type DecimalsOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
        
    
    [<FunctionOutput>]
    type TotalSupplyOutputDTO() =
        inherit FunctionOutputDTO() 
            [<Parameter("uint256", "out", 1)>]
            member val public Out = Unchecked.defaultof<BigInteger> with get, set
    

