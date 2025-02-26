﻿using System;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Uniswap.Infrastructure;

namespace WebSocket.Uniswap.Services
{
    internal interface IWebSocketConnectionsService
    {
        void AddConnection(WebSocketConnection connection);

        void RemoveConnection(Guid connectionId);

        Task SendToAllAsync(string message, CancellationToken cancellationToken);
    }
}
