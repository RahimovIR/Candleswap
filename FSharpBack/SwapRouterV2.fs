namespace RedDuck.Candleswap.Candles

open System
open System.Numerics
open System.Linq
open Nethereum.RPC.Eth.DTOs
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Contracts
open Contracts.UniswapV2Router.ContractDefinition


module SwapRouterV2 =
    let router01Address = "0xf164fC0Ec4E93095b804a4795bBe1e041497b92a"
    let router02Address = "0x7a250d5630b4cf539739df2c5dacb4c659f2488d"
    let addresses = [ router01Address; router02Address ]


    let swapExactTokensForTokensId = "0x38ed1739"
    let swapTokensForExactTokensId = "0x8803dbee"
    let swapExactETHForTokensId = "0x7ff36ab5"
    let swapTokensForExactETHId = "0x4a25d94a"
    let swapExactTokensForETHId = "0x18cbafe5"
    let swapETHForExactTokensId = "0xfb3bdb41"
    let swapExactTokensForTokensSupportingFeeOnTransferTokensId = "0x5c11d795"
    let swapExactETHForTokensSupportingFeeOnTransferTokensId = "0xb6f9de95"
    let swapExactTokensForETHSupportingFeeOnTransferTokensId = "0x791ac947"

    [<Event("Swap")>]
    type SwapEventDTO() =
        inherit EventDTO()

        [<Parameter("address", "sender", 1, true)>]
        member val Sender = Unchecked.defaultof<string> with get, set

        [<Parameter("uint", "amount0In", 2, false)>]
        member val Amount0In = Unchecked.defaultof<BigInteger> with get, set

        [<Parameter("uint", "amount1In", 3, false)>]
        member val Amount1In = Unchecked.defaultof<BigInteger> with get, set

        [<Parameter("uint", "amount0Out", 4, false)>]
        member val Amount0Out = Unchecked.defaultof<BigInteger> with get, set

        [<Parameter("uint", "amount1Out", 5, false)>]
        member val Amount1Out = Unchecked.defaultof<BigInteger> with get, set

        [<Parameter("address", "to", 6, true)>]
        member val To = Unchecked.defaultof<string> with get, set

    let getTokensAndAmountsFromRouter (func: FunctionMessage) (event: SwapEventDTO) transactionInput = 
        let getAmount amount0 amount1 =
            if amount0 = 0I then amount1 else amount0

        let amountIn = getAmount event.Amount0In event.Amount1In
        let amountOut = getAmount event.Amount0Out event.Amount1Out

        if func :? SwapExactTokensForTokensFunction
        then let decoded = (new SwapExactTokensForTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapTokensForExactTokensFunction
        then let decoded = (new SwapTokensForExactTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapExactETHForTokensFunction
        then let decoded = (new SwapExactETHForTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapTokensForExactETHFunction
        then let decoded = (new SwapTokensForExactETHFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapExactTokensForETHFunction
        then let decoded = (new SwapExactTokensForETHFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapETHForExactTokensFunction
        then let decoded = (new SwapETHForExactTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction
        then let decoded = (new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapExactETHForTokensSupportingFeeOnTransferTokensFunction
        then let decoded = (new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else if func :? SwapExactTokensForETHSupportingFeeOnTransferTokensFunction
        then let decoded = (new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction()).DecodeInput(transactionInput)
             (decoded.Path.First(), decoded.Path.Last(), amountIn, amountOut)
        else ("", "", 0I, 0I)

    let getInfoFromRouter (transaction: Transaction) (transactionReceipt: TransactionReceipt) =
        let swapEvents = transactionReceipt.Logs.DecodeAllEvents<SwapEventDTO>()
        if transaction.Input.Contains(swapExactTokensForTokensId) 
        then getTokensAndAmountsFromRouter (new SwapExactTokensForTokensFunction()) swapEvents.[0].Event 
                                                                                    transaction.Input
        else if transaction.Input.Contains(swapTokensForExactTokensId) 
        then getTokensAndAmountsFromRouter (new SwapTokensForExactTokensFunction()) swapEvents.[0].Event  
                                                                                    transaction.Input
        else if transaction.Input.Contains(swapExactETHForTokensId)
        then getTokensAndAmountsFromRouter (new SwapExactETHForTokensFunction())swapEvents.[0].Event transaction.Input
        else if transaction.Input.Contains(swapTokensForExactETHId) 
        then getTokensAndAmountsFromRouter (new SwapTokensForExactETHFunction()) swapEvents.[0].Event transaction.Input
        else if transaction.Input.Contains(swapExactTokensForETHId) 
        then getTokensAndAmountsFromRouter (new SwapExactTokensForETHFunction()) swapEvents.[0].Event  transaction.Input
        else if transaction.Input.Contains(swapETHForExactTokensId) 
        then getTokensAndAmountsFromRouter (new SwapETHForExactTokensFunction()) swapEvents.[0].Event  transaction.Input
        else if transaction.Input.Contains(swapExactTokensForTokensSupportingFeeOnTransferTokensId) 
        then getTokensAndAmountsFromRouter (new SwapExactTokensForTokensSupportingFeeOnTransferTokensFunction()) 
                                           swapEvents.[0].Event transaction.Input
        else if transaction.Input.Contains(swapExactETHForTokensSupportingFeeOnTransferTokensId) 
        then getTokensAndAmountsFromRouter (new SwapExactETHForTokensSupportingFeeOnTransferTokensFunction()) 
                                           swapEvents.[0].Event transaction.Input
        else if transaction.Input.Contains(swapExactTokensForETHSupportingFeeOnTransferTokensId) 
        then getTokensAndAmountsFromRouter (new SwapExactTokensForETHSupportingFeeOnTransferTokensFunction())
                                           swapEvents.[0].Event transaction.Input
        else ("", "", 0I, 0I)
