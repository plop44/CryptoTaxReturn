using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class DustLog
    {
        [JsonProperty("tranId")] public long TranId { get; set; }
        [JsonProperty("serviceChargeAmount")] public decimal ServiceChargeAmount { get; set; }
        [JsonProperty("uid")] public string Uid { get; set; }
        [JsonProperty("amount")] public decimal Amount { get; set; }
        [JsonProperty("operateTime")] public string OperateTime { get; set; }
        [JsonProperty("transferedAmount")] public decimal TransferedAmount { get; set; }
        [JsonProperty("fromAsset")] public string FromAsset { get; set; }
        public long OperateTimeLong => DateTime.Parse(OperateTime).ToUnixDateTime();
    }
}