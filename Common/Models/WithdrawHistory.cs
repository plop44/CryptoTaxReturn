using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class WithdrawHistory
    {
        [JsonProperty("withdrawList")]
        public List<Withdraw> WithdrawList { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }
    }
}
