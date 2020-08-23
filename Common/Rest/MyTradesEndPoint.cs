using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json;

namespace Common.Rest
{
    public sealed class MyTradesEndPoint
    {
        private readonly BinanceRestClient _restClient;

        //symbol STRING  YES
        //startTime   LONG NO
        //endTime LONG    NO
        //fromId  LONG NO  TradeId to fetch from.Default gets most recent trades.
        //limit INT NO Default 500; max 1000.
        //recvWindow LONG    NO The value cannot be greater than 60000
        //timestamp LONG    YES

        public MyTradesEndPoint(BinanceRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<IList<Trade>> GetMyTrades(string symbol, int id)
        {
            var parameters = $"symbol={symbol}&limit=1000&fromId={id}";

            parameters = _restClient.AddTimestampAndSign(parameters);
            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v3/myTrades", parameters);

            return JsonConvert.DeserializeObject<IList<Trade>>(reply);
        }
    }
}