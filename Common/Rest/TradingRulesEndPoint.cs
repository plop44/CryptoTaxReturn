using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json;

namespace Common.Rest
{
    public sealed class TradingRulesEndPoint
    {
        private readonly BinanceRestClient _restClient;

        public TradingRulesEndPoint(BinanceRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<TradingRules> GetTradingRules()
        {
            var reply =  await _restClient.CallAsync(HttpMethod.Get, "/api/v1/exchangeInfo");
            return JsonConvert.DeserializeObject<TradingRules>(reply);
        }
    }
}