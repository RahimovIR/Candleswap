using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using WebSocket.Uniswap.Models;

namespace WebSocket.Uniswap.Infrastructure
{
    internal class WebSocketConnection
    {
        #region Fields
        private System.Net.WebSockets.WebSocket _webSocket;
        private int _receivePayloadBufferSize;
        #endregion

        #region Properties
        public Guid Id { get; } = Guid.NewGuid();

        public WebSocketCloseStatus? CloseStatus { get; private set; } = null;

        public string CloseStatusDescription { get; private set; } = null;
        #endregion

        #region Events
        public event EventHandler<string> ReceiveText;

        public event EventHandler<byte[]> ReceiveBinary;

        public event EventHandler<string> ReceiveCandleUpdate;

        #endregion

        #region Constructor
        public WebSocketConnection(System.Net.WebSockets.WebSocket webSocket, int receivePayloadBufferSize)
        {
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _receivePayloadBufferSize = receivePayloadBufferSize;
        }
        #endregion

        #region Methods
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

        public async Task ReceiveMessagesUntilCloseAsync()
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
                        else if (webSocketMessage.Contains("candles"))
                            await OnReceiveCandles(webSocketMessage);
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

        private async Task OnReceiveCandles(string webSocketMessage)
        {
            var webSocketRequest = JsonConvert.DeserializeObject<CandleUpdate>(webSocketMessage);
            var arrayKeyParam = webSocketRequest.KeyParam.Split(':');
            int resolution = arrayKeyParam[1] switch 
            {
                "1m" => 60,
                _ => 10
            };

            CandleEvent.SubscribeCandles(arrayKeyParam[2], OnCandleUpdateReceived, resolution);

            ReceiveText?.Invoke(this, webSocketMessage);
        }

        #endregion
    }
}
