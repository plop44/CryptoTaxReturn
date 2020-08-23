using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class DepositHistory
    {
        [JsonProperty("depositList")]
        public List<Deposit> DepositList { get; set; }
        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
