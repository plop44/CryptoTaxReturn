using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Models;
using Newtonsoft.Json;

namespace Common.Rest
{
    public sealed class DepositHistoryEndPoint
    {
        private readonly BinanceRestClient _restClient;

        //asset STRING  NO
        //    status  INT NO	0(0:Email Sent,1:Cancelled 2:Awaiting Approval 3:Rejected 4:Processing 5:Failure 6Completed)
        //startTime LONG    NO
        //    endTime LONG NO
        //recvWindow LONG    NO
        //    timestamp   LONG YES

        public DepositHistoryEndPoint(BinanceRestClient restClient)
        {
            _restClient = restClient;
        }

        public async Task<WithdrawHistory> GetWithdrawHistory(DateTime startDate)
        {
            var toReturn = new WithdrawHistory {Success = true, WithdrawList = new List<Withdraw>()};

            while (true)
            {
                if (startDate > DateTime.Today)
                    break;

                DateTime endDate = startDate.AddDays(90);
                var parameters = $"startTime={startDate.ToUnixDateTime()}&endTime={endDate.ToUnixDateTime()}";
                parameters = _restClient.AddTimestampAndSign(parameters);
                var result = await _restClient.CallAsync(HttpMethod.Get, "/wapi/v3/withdrawHistory.html", parameters);
                var resultTyped = JsonConvert.DeserializeObject<WithdrawHistory>(result);

                if (!resultTyped.Success)
                    throw new Exception();

                toReturn.WithdrawList.AddRange(resultTyped.WithdrawList??Enumerable.Empty<Withdraw>());
                startDate = endDate.AddDays(1);
            }

            return toReturn;
        }

        public async Task<DepositHistory> GetDepositHistory(DateTime startDate)
        {
            var toReturn = new DepositHistory {Success = true, DepositList = new List<Deposit>()};

            while (true)
            {
                if (startDate > DateTime.Today)
                    break;

                DateTime endDate = startDate.AddDays(90);
                var parameters = $"startTime={startDate.ToUnixDateTime()}&endTime={endDate.ToUnixDateTime()}";
                parameters = _restClient.AddTimestampAndSign(parameters);
                var result = await _restClient.CallAsync(HttpMethod.Get, "/wapi/v3/depositHistory.html", parameters);

                var resultTyped = JsonConvert.DeserializeObject<DepositHistory>(result);

                if (!resultTyped.Success)
                    throw new Exception();

                toReturn.DepositList.AddRange(resultTyped.DepositList??Enumerable.Empty<Deposit>());
                startDate = endDate.AddDays(1);
            }

            return toReturn;
        }

        public async Task<DividendHistory> GetDividendHistory(DateTime startDate)
        {
            var toReturn = new DividendHistory {Total = 0, Rows = new List<Dividend>()};

            while (true)
            {
                if (startDate > DateTime.Today)
                    break;

                DateTime endDate = startDate.AddDays(10);
                var parameters = $"recvWindow=60000&startTime={startDate.ToUnixDateTime()}&endTime={endDate.ToUnixDateTime()}";
                parameters = _restClient.AddTimestampAndSign(parameters);
                var result = await _restClient.CallAsync(HttpMethod.Get, "/sapi/v1/asset/assetDividend", parameters);

                var resultTyped = JsonConvert.DeserializeObject<DividendHistory>(result);

                if(resultTyped.Total == 20)
                    throw new Exception("Binance bug is returning 20 rows only, we might miss entries.");

                toReturn.Rows.AddRange(resultTyped.Rows ?? Enumerable.Empty<Dividend>());
                toReturn.Total += resultTyped.Rows?.Count ?? 0;
                startDate = endDate;
            }

            return toReturn;
        }

        public async Task<DustHistory> GetDustHistory()
        {
            var parameters = string.Empty;
            parameters = _restClient.AddTimestampAndSign(parameters);
            var result = await _restClient.CallAsync(HttpMethod.Get, "/wapi/v3/userAssetDribbletLog.html", parameters);

            return JsonConvert.DeserializeObject<DustHistory>(result);
        }
    }
}