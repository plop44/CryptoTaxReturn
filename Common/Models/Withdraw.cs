using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class Withdraw
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("asset")] public string Asset { get; set; }
        [JsonProperty("amount")] public decimal Amount { get; set; }
        [JsonProperty("transactionFee")] public decimal TransactionFee { get; set; }
        [JsonProperty("address")] public string Address { get; set; }
        [JsonProperty("txId")] public string TxId { get; set; }
        [JsonProperty("applyTime")] public long ApplyTime { get; set; }
        [JsonProperty("status")] public int Status { get; set; }
        [JsonProperty("addressTag")] public string AddressTag { get; set; }
        public DateTime TimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(ApplyTime).DateTime;
    }
}