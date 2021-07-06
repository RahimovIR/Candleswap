using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocket.Uniswap.Infrastructure;

namespace Test.Uniswap
{
    [TestClass]
    public class WebSocketTest
    {
        private const string webSocketURL = "wss://localhost:5001/socket";

        [TestMethod]
        public async Task GetCandles_PassWhenAny()
        {
            var pairId = "0x000ea4a83acefdd62b1b43e9ccc281f442651520";
            var resolutionTime = TimeSpan.FromSeconds(5);
            var cancelDelay = new CancellationTokenSource();

            var responseList = new List<string>();
            CandleEvent.SubscribeCandles(
                     pairId,
                     c =>
                     {
                         Console.WriteLine(c);
                         responseList.Add(c);
                         // stop waiting if a response came
                         cancelDelay.Cancel();
                     },
                     (int)resolutionTime.TotalSeconds);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancelDelay.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Awaiting was canceled: a response came.");
            }
            finally
            {
                CandleEvent.UnsubscribeCandles(pairId, (int)resolutionTime.TotalSeconds);
            }
            Assert.IsTrue(responseList.Any());
        }

        [TestMethod]
        public async Task GetCandles_PassWhenCandleReceived()
        {
            var pairId = "0x000ea4a83acefdd62b1b43e9ccc281f442651520";
            var resolutionTime = TimeSpan.FromSeconds(5);
            var cancelDelay = new CancellationTokenSource();

            var candlesList = new List<string>();
            CandleEvent.SubscribeCandles(
                     pairId,
                     c =>
                     {
                         Console.WriteLine(c);
                         if (c.Contains("_open"))
                         {
                             candlesList.Add(c);

                             // stop waiting if a response came
                             cancelDelay.Cancel();
                         }
                     },
                     (int)resolutionTime.TotalSeconds);

            TimeSpan delay = TimeSpan.FromMinutes(5);

            try
            {
                await Task.Delay(delay, cancelDelay.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Awaiting was canceled: a candle came.");
            }
            finally
            {
                CandleEvent.UnsubscribeCandles(pairId, (int)resolutionTime.TotalSeconds);
            }
            Assert.IsTrue(candlesList.Any(), $"No candles received for {delay}");
        }

        [TestMethod]
        public async Task GetHistoricalCandles_PassWhenAny()
        {
            var pairId = "0x000ea4a83acefdd62b1b43e9ccc281f442651520";
            var resolutionTime = TimeSpan.FromSeconds(5);
            var cancelDelay = new CancellationTokenSource();

            var responseList = new List<string>();
            CandleEvent.SubscribeHistoricalCandles(
                     pairId,
                     c =>
                     {
                         Console.WriteLine(c);
                         responseList.Add(c);
                         // stop waiting if a response came
                         cancelDelay.Cancel();
                     },
                     (int)resolutionTime.TotalSeconds);

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancelDelay.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Awaiting was canceled: a response came.");
            }
            finally
            {
                CandleEvent.UnsubscribeCandles(pairId, (int)resolutionTime.TotalSeconds);
            }
            Assert.IsTrue(responseList.Any());
        }

        [TestMethod]
        public async Task GetHistoricalCandles_PassWhenCandleReceived()
        {
            var pairId = "0x000ea4a83acefdd62b1b43e9ccc281f442651520";
            var resolutionTime = TimeSpan.FromMinutes(5);
            var cancelDelay = new CancellationTokenSource();

            var candlesList = new List<string>();
            CandleEvent.SubscribeHistoricalCandles(
                     pairId,
                     c =>
                     {
                         Console.WriteLine(c);
                         if (c.Contains("_open"))
                         {
                             candlesList.Add(c);

                             // stop waiting if a response came
                             cancelDelay.Cancel();
                         }
                     },
                     (int)resolutionTime.TotalSeconds);

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancelDelay.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Awaiting was canceled: a candle came.");
            }
            finally
            {
                CandleEvent.UnsubscribeCandles(pairId, (int)resolutionTime.TotalSeconds);
            }
            Assert.IsTrue(candlesList.Any());
        }

        [TestMethod]
        public async Task GetCandlesFromWebsocket()
        {
            var buffer = new byte[1024 * 4];
            var responseList = new List<string>();
            var client = await CreateConnectWithLocalWebSocket();

            string subscribeRequest = @"{
                            ""event"": ""subscribe"",
                            ""channel"": ""candles"",
                            ""key"": ""trade:10s:0x000ea4a83acefdd62b1b43e9ccc281f442651520""
                            }";
            var bytes = Encoding.UTF8.GetBytes(subscribeRequest);
            await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            var attemptsCount = 0;
            var cancelReceive = new CancellationTokenSource();

            while (!responseList.Any() && attemptsCount < 10)
            {
                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cancelReceive.Token);

                string response = Encoding.UTF8.GetString(buffer, 0, result.Count).ToUpperInvariant();
                Console.WriteLine(response);
                if (response.Contains("NO SWAPS") || response.Contains("_OPEN"))
                {
                    responseList.Add(response.TrimEnd('\0'));
                }
                attemptsCount++;
            }
            cancelReceive.Cancel();
            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            Assert.IsTrue(responseList.Any());
        }

        [TestMethod]
        public async Task PlayPingPongWebSocketTest()
        {
            var buffer = new byte[1024 * 4];

            const string message = "PING";
            const string responseFromWebSocket = "PONG";

            string response = string.Empty;

            var client = await CreateConnectWithLocalWebSocket();

            for (int i = 0; i < 5; i++)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                response = Encoding.UTF8.GetString(buffer, 0, result.Count).TrimEnd('\0').ToUpper();
            }

            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

            Assert.AreEqual(responseFromWebSocket, response);
        }

        [TestMethod]
        public async Task GetUniswapHeartbeatTest()
        {
            bool isCompleted = false;
            var buffer = new byte[1024 * 4];
            int actualCounterUniswapHeartbeat = 0;
            int expectedCounterUniswapHeartbeat = 10;

            try
            {
                var client = await CreateConnectWithLocalWebSocket();

                while (!isCompleted)
                {
                    var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var webSocketMessage = Encoding.UTF8.GetString(buffer, 0, result.Count).TrimEnd('\0');
                        actualCounterUniswapHeartbeat = webSocketMessage == "Uniswap Heartbeat" ? ++actualCounterUniswapHeartbeat : actualCounterUniswapHeartbeat;

                        Assert.AreEqual("Uniswap Heartbeat", webSocketMessage);

                        if (actualCounterUniswapHeartbeat >= 10)
                            isCompleted = true;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        break;
                    }
                }

                Assert.AreEqual(expectedCounterUniswapHeartbeat, actualCounterUniswapHeartbeat);
                Assert.IsTrue(isCompleted);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Helpers

        private async Task<ClientWebSocket> CreateConnectWithLocalWebSocket()
        {
            var client = new ClientWebSocket();
            await client.ConnectAsync(new Uri(webSocketURL), CancellationToken.None);
            return client;
        }

        #endregion
    }
}