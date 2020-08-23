using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json.Linq;

namespace Common.Rest
{
    public sealed class CandlesticksEndPoint
    {
        private readonly BinanceRestClient _restClient;

        //Name Type    Mandatory Description
        //symbol STRING  YES
        //interval    ENUM YES
        //startTime LONG    NO
        //endTime LONG NO
        //limit INT NO Default 500; max 1000.

        public CandlesticksEndPoint(BinanceRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<Candlestick[]> GetCandlesticks(string symbol, TimeInterval timeInterval, long? start = null, long? end = null, int? limit = null)
        {
            var parameters = $"symbol={symbol}&interval={timeInterval.GetDescription()}";

            if (start.HasValue)
                parameters += $"&startTime={start.Value}";
            if (end.HasValue)
                parameters += $"&endTime={end.Value}";
            if (limit.HasValue)
                parameters += $"&limit={limit.Value}";

            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v3/klines", parameters);
            return Convert(reply);
        }

        private static Candlestick[] Convert(string input)
        {
            //1499040000000,      // Open time
            //"0.01634790",       // Open
            //"0.80000000",       // High
            //"0.01575800",       // Low
            //"0.01577100",       // Close
            //"148976.11427815",  // Volume
            //1499644799999,      // Close time
            //"2434.19055334",    // Quote asset volume
            //308,                // Number of trades
            //"1756.87402397",    // Taker buy base asset volume
            //"28.46694368",      // Taker buy quote asset volume
            //"17928899.62484339" // Ignore.
            return JArray.Parse(input).Select(item => new Candlestick(
                item[0].Value<long>(),
                item[1].Value<decimal>(),
                item[2].Value<decimal>(),
                item[3].Value<decimal>(),
                item[4].Value<decimal>(),
                item[5].Value<decimal>(),
                item[6].Value<long>(),
                item[7].Value<decimal>(),
                item[8].Value<int>(),
                item[9].Value<decimal>(),
                item[10].Value<decimal>()
            )).ToArray();
        }
    }
}