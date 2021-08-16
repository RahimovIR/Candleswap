using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Nethereum.Web3;
using RedDuck.Candleswap.Candles.CSharp;
using WebSocket.Uniswap.Middlewares;
using WebSocket.Uniswap.Services;

namespace WebSocket.Uniswap
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddWebSocketConnections();

            services.AddSingleton<IHostedService, HeartbeatService>();

            services.AddSingleton<ISqlConnectionProvider, SqlConnectionProvider>();
            services.AddTransient<ILogicService, LogicService>();
            services.AddSingleton<IWeb3>(
                new Web3("https://mainnet.infura.io/v3/dc6ea0249f9e4c1187bbcaf0fbe0ff6e"));

            services.AddTransient<ICandleStorageService, CandleStorageService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebSocket.Uniswap", Version = "v1" });
            });

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
                loggingBuilder.AddAzureWebAppDiagnostics();
            });

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSocket.Uniswap v1"));

            WebSocketConnectionsOptions webSocketConnectionsOptions = new WebSocketConnectionsOptions
            {
                //AllowedOrigins = new HashSet<string> { "wss://localhost:5001/socket" },
                SendSegmentSize = 4 * 1024
            };

            app.UseWebSockets();

            app.MapWebSocketConnections("/socket", webSocketConnectionsOptions);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
