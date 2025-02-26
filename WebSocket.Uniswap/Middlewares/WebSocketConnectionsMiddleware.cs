﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RedDuck.Candleswap.Candles.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Uniswap.Infrastructure;
using WebSocket.Uniswap.Services;

namespace WebSocket.Uniswap.Middlewares
{
    internal class WebSocketConnectionsMiddleware
    {
        private readonly WebSocketConnectionsOptions _options;
        private readonly IWebSocketConnectionsService _connectionsService;
        private readonly ILogicService _logic;
        private readonly ICandleStorageService _candleStorageService;
        private readonly ILogger<WebSocketConnection> _logger;

        public WebSocketConnectionsMiddleware(
            RequestDelegate next, 
            WebSocketConnectionsOptions options, 
            IWebSocketConnectionsService connectionsService,
            ILogicService logic,
            ICandleStorageService candleStorageService,
            ILogger<WebSocketConnection> logger
            )
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionsService = connectionsService ?? throw new ArgumentNullException(nameof(connectionsService));
            _logic = logic;
            _candleStorageService = candleStorageService;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                if (ValidateOrigin(context))
                {
                    System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    WebSocketConnection webSocketConnection = new WebSocketConnection(webSocket, _options.ReceivePayloadBufferSize);

                    async void OnReceiveText(object sender, string message)
                    {
                        await webSocketConnection.SendAsync(message, CancellationToken.None);
                    }
                    async void OnReceiveCandleUpdate(object sender, string candle)
                    {
                        await webSocketConnection.SendAsync(candle, CancellationToken.None);
                    }

                    webSocketConnection.ReceiveText += OnReceiveText;
                    webSocketConnection.ReceiveCandleUpdate += OnReceiveCandleUpdate;
                    _connectionsService.AddConnection(webSocketConnection);

                    var cancelReceiveMessages = new CancellationTokenSource();

                    await webSocketConnection.ReceiveMessagesUntilCloseAsync(_logic, _candleStorageService, _logger);

                    if (webSocketConnection.CloseStatus.HasValue)
                    {
                        await webSocket.CloseAsync(webSocketConnection.CloseStatus.Value, webSocketConnection.CloseStatusDescription, CancellationToken.None);
                    }

                    webSocketConnection.ReceiveCandleUpdate -= OnReceiveCandleUpdate;
                    webSocketConnection.ReceiveText -= OnReceiveText;
                    _connectionsService.RemoveConnection(webSocketConnection.Id);

                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private bool ValidateOrigin(HttpContext context)
        {
            return (_options.AllowedOrigins == null) || (_options.AllowedOrigins.Count == 0) || (_options.AllowedOrigins.Contains(context.Request.Headers["Origin"].ToString()));
        }
    }
}
