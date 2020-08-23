using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json;

namespace Common.Rest
{
    public class AccountInfoEndPoint
    {
        private readonly BinanceRestClient _restClient;

        public AccountInfoEndPoint(BinanceRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<AccountInfo> GetAccountInfo()
        {
            var parameters = $"recvWindow=60000";
            parameters = _restClient.AddTimestampAndSign(parameters);

            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v3/account", parameters);
            return JsonConvert.DeserializeObject<AccountInfo>(reply);
        }
    }
}