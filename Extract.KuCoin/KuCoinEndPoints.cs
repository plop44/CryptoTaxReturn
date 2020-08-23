using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json;

namespace Extract.KuCoin
{
    public class KuCoinEndPoints
    {
        private readonly KuCoinRestClient _restClient;

        public KuCoinEndPoints(KuCoinRestClient restClient)
        {
            _restClient = restClient;
        }

        private async Task<KuCoinTrades> GetTrades(DateTime start, DateTime end)
        {
            var parameters = $"tradeType=TRADE&startAt={start.ToUnixDateTime()}&endAt={end.ToUnixDateTime()}&";
            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v1/fills", parameters);
            return JsonConvert.DeserializeObject<ApiResponse<KuCoinTrades>>(reply).Data;
        }

        public async Task<KuCoinDeposits> GetDeposit()
        {
            var parameters = "status=SUCCESS";
            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v1/deposits", parameters);
            return JsonConvert.DeserializeObject<ApiResponse<KuCoinDeposits>>(reply).Data;
        }

        public async Task<KuCoinWithdrawals> GetWithdrawals()
        {
            var parameters = "status=SUCCESS";
            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v1/withdrawals", parameters);
            return JsonConvert.DeserializeObject<ApiResponse<KuCoinWithdrawals>>(reply).Data;
        }

        public async Task<KcBalance[]> GetBalances()
        {
            var reply = await _restClient.CallAsync(HttpMethod.Get, "/api/v1/accounts", null);
            return JsonConvert.DeserializeObject<ApiResponse<KcBalance[]>>(reply).Data;
        }

        public async Task<ImmutableArray<KuCoinTrade>> GetAllTrades(DateTime starDateTime)
        {
            var toReturn = new List<KuCoinTrade>();
            while (starDateTime<DateTime.Today)
            {
                var trades  = await GetTrades(starDateTime, starDateTime.AddDays(7));
                toReturn.AddRange(trades.Items);
                starDateTime = starDateTime.AddDays(7);
            }

            return toReturn.ToImmutableArray();
        }
    }
}