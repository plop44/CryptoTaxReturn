using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class DustRow
    {
        [JsonProperty("transfered_total")] public decimal TransferedTotal { get; set; }
        [JsonProperty("service_charge_total")] public decimal ServiceChargeTotal { get; set; }
        [JsonProperty("tran_id")] public long TranId { get; set; }
        [JsonProperty("logs")] public DustLog[] Logs { get; set; }
        [JsonProperty("operate_time")] public string OperateTime { get; set; }
         public long OperateTimeLong => OperateTimeDateTime.ToUnixDateTime();
        public DateTime OperateTimeDateTime => DateTime.Parse(OperateTime);
    }
}