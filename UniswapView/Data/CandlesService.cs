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
        public string CreateGetCandlesUri(string symbol, int periodSeconds, 
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

            return "https://localhost:5001/api/candles" + ToQueryString(queryParams);
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
