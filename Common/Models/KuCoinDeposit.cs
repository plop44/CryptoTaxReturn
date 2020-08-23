using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class KuCoinDeposit
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("memo")]
        public string Memo { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("isInner")]
        public bool IsInner { get; set; }

        [JsonProperty("walletTxId")]
        public string WalletTxId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public object UpdatedAt { get; set; }
    }
}