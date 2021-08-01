using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace WebSocket.Uniswap.Middlewares
{
    internal static class WebSocketConnectionsMiddlewareExtensions
    {
        public static IApplicationBuilder MapWebSocketConnections(this IApplicationBuilder app, PathString pathMatch, WebSocketConnectionsOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.Map(pathMatch, branchedApp => branchedApp.UseMiddleware<WebSocketConnectionsMiddleware>(options));
        }
    }
}
