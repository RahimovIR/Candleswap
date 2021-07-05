using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
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
        public async Task GetCandles()
        {
            static void LogResponse(string c)
            {
                Console.WriteLine(c); 
                Debug.WriteLine(c); 
            }
            var pairId = "0xb4e16d0168e52d35cacd2c6185b44281ec28c9dc";
            var resolutionTime = TimeSpan.FromSeconds(10);

            CandleEvent.SubscribeCandles(
                     pairId,
                     c => {
                         Console.WriteLine(c);
                         Debug.WriteLine(c);
                     },
                     (int)resolutionTime.TotalSeconds);

            await Task.Delay(TimeSpan.FromSeconds(20));
            CandleEvent.UnsubscribeCandles(pairId, (int)resolutionTime.TotalSeconds);
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