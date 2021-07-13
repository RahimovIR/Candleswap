using Newtonsoft.Json.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace UniswapView.Data
{
    public class CandlesService
    {
        public async Task<FSharpBack.DBCandle[]> GetCandlesAsync(string symbol, int periodSeconds, 
            long? startTime = default, long? endTime = default, int? limit = default)
        {
            var client = new HttpClient();
            var queryParams = new NameValueCollection
            {
                { "symbol", symbol },
                { "periodSeconds", periodSeconds.ToString() }
            };
            if (startTime.HasValue)
            {
                queryParams.Add("startTime", startTime.ToString());
            }
            if (endTime.HasValue)
            {
                queryParams.Add("endTime", endTime.ToString());
            }
            if (limit.HasValue)
            {
                queryParams.Add("limit", limit.ToString());
            }
            var request = new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://localhost:5001/api/candles" + ToQueryString(queryParams)),
            };

            var response = await client.SendAsync(request);
            var responseToken = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync(responseToken, typeof(FSharpBack.DBCandle[])) 
                as FSharpBack.DBCandle[];
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (
                from key in nvc.AllKeys
                from value in nvc.GetValues(key)
                select string.Format(
            "{0}={1}",
            HttpUtility.UrlEncode(key),
            HttpUtility.UrlEncode(value))
                ).ToArray();
            return "?" + string.Join("&", array);
        }
    }
}
