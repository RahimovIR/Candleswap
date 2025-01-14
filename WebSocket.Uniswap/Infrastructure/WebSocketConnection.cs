﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using WebSocket.Uniswap.Models;
using RedDuck.Candleswap.Candles.CSharp;
using Microsoft.Extensions.Logging;

namespace WebSocket.Uniswap.Infrastructure
{
    internal class WebSocketConnection
    {
        private System.Net.WebSockets.WebSocket _webSocket;
        private int _receivePayloadBufferSize;

        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;

        public event EventHandler<string> ReceiveText;

        public event EventHandler<byte[]> ReceiveBinary;

        public event EventHandler<string> ReceiveCandleUpdate;

        public WebSocketConnection(System.Net.WebSockets.WebSocket webSocket, int receivePayloadBufferSize)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _receivePayloadBufferSize = receivePayloadBufferSize;
        }

        public Task SendAsync(string message, CancellationToken cancellationToken)
        {
            return _webSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message),
                                                                  offset: 0,
                                                                  count: message.Length),
                                                                  messageType: WebSocketMessageType.Text,
                                                                  endOfMessage: true,
                                                                  cancellationToken: cancellationToken);
        }

        public Task SendAsync(byte[] message, CancellationToken cancellationToken)
        {
            return _webSocket.SendAsync(buffer: message,
                                        messageType: WebSocketMessageType.Text,
                                        endOfMessage: true,
                                        cancellationToken: cancellationToken);
        }

        public async Task ReceiveMessagesUntilCloseAsync(ILogicService logic, ICandleStorageService candleStorage,
                                                         ILogger<WebSocketConnection> logger)
        {
            try
            {
                byte[] receivePayloadBuffer = new byte[_receivePayloadBufferSize];
                WebSocketReceiveResult webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                while (webSocketReceiveResult.MessageType != WebSocketMessageType.Close)
                {
                    if (webSocketReceiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        var webSocketMessage = Encoding.UTF8.GetString(receivePayloadBuffer).TrimEnd('\0').ToUpper();
                        if (webSocketMessage == "PING")
                        {
                            OnReceivePingPong(webSocketMessage);
                        }
                        else
                            OnReceiveBinary(receivePayloadBuffer);
                    }
                    else
                    {
                        var webSocketMessage = Encoding.UTF8.GetString(receivePayloadBuffer).TrimEnd('\0');
                        if (webSocketMessage == "PING")
                        {
                            OnReceivePingPong(webSocketMessage);
                        }
                        else if (webSocketMessage.Contains("candles") || webSocketMessage.Contains("historicalCandles"))
                        {
                            if(webSocketMessage.Contains("unsubscribe"))
                                await OnReceiveCandlesUnsubscribeRequest(candleStorage, webSocketMessage);
                            else if(webSocketMessage.Contains("subscribe"))
                                await OnReceiveCandlesSubscribeRequest(logic, candleStorage,  webSocketMessage);
                        }
                        else
                            OnReceiveText(webSocketMessage);
                    }

                    webSocketReceiveResult = await _webSocket.ReceiveAsync(new ArraySegment<byte>(receivePayloadBuffer), CancellationToken.None);
                }

                CloseStatus = webSocketReceiveResult.CloseStatus.Value;
                CloseStatusDescription = webSocketReceiveResult.CloseStatusDescription;
            }
            catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {

            }
            catch(Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }

        private void OnReceiveText(string webSocketMessage)
        {
            ReceiveText?.Invoke(this, webSocketMessage);
        }

        private void OnReceiveBinary(byte[] webSocketMessage)
        {
            ReceiveBinary?.Invoke(this, webSocketMessage);
        }

        private void OnReceivePingPong(string _)
        {
            var webSocketMessage = "PONG";
            ReceiveText?.Invoke(this, webSocketMessage);
        }

        private void OnCandleUpdateReceived(string candle)
        {
            ReceiveCandleUpdate?.Invoke(this, candle);
        }

        private async Task OnReceiveCandlesSubscribeRequest(ILogicService logic, ICandleStorageService candleStorage, string webSocketMessage)
        {
            var webSocketRequest = JsonConvert.DeserializeObject<CandleUpdate>(webSocketMessage);
            var arrayKeyParam = webSocketRequest.KeyParam.Split(':');
            int resolution = GetResolution(arrayKeyParam[1]);

            if (arrayKeyParam.Length > 3)
            {
                await CandleEvent.SubscribeCandlesAsync(logic, candleStorage, arrayKeyParam[2], arrayKeyParam[3],
                                                        OnCandleUpdateReceived, resolution, webSocketRequest.Channel);   
            }
            else
            {
                //TODO: Subscribe candles with pairId
            }

            ReceiveText?.Invoke(this, webSocketMessage);
        }

        private async Task OnReceiveCandlesUnsubscribeRequest(ICandleStorageService candleStorage, string webSocketMessage)
        {
            var webSocketRequest = JsonConvert.DeserializeObject<CandleUpdate>(webSocketMessage);
            var arrayKeyParam = webSocketRequest.KeyParam.Split(':');
            int resolution = GetResolution(arrayKeyParam[1]);
            if (arrayKeyParam.Length > 3)
            {
                await CandleEvent.UnsubscribeCandlesAsync(candleStorage, arrayKeyParam[2], arrayKeyParam[3], resolution, webSocketRequest.Channel);
            }
            else
            {
                //TODO: Unsubscribe candles with pairId
            }

            ReceiveText?.Invoke(this, webSocketMessage);
        }

        private static int GetResolution(string arrayKeyParam)
        {
            return arrayKeyParam switch
            {
                "1h" => 3600,
                "30m" => 1800,
                "5m" => 300,
                "1m" => 60,
                "30s" => 30,
                "10s" => 10,
                _ => 10
            };
        }
    }
}
