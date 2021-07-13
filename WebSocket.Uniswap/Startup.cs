using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using WebSocket.Uniswap.Middlewares;
using WebSocket.Uniswap.Services;

namespace WebSocket.Uniswap
{
    public class Startup
    {
        const string CorsAllowCrossOrigins = "access-control-allow-origin";
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebSocket.Uniswap", Version = "v1" });
            });

            services.AddCors(options =>
            {
                options.AddPolicy(CorsAllowCrossOrigins,
                    builder =>
                    {
                        builder.WithOrigins("*")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebSocket.Uniswap v1"));
            }

            WebSocketConnectionsOptions webSocketConnectionsOptions = new WebSocketConnectionsOptions
            {
                //AllowedOrigins = new HashSet<string> { "wss://localhost:5001/socket" },
                SendSegmentSize = 4 * 1024
            };

            app.UseWebSockets();

            app.MapWebSocketConnections("/socket", webSocketConnectionsOptions);

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(CorsAllowCrossOrigins);

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
