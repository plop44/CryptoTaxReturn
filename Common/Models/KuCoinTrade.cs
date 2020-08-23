using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class KuCoinTrade
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("tradeId")]
        public string TradeId { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("counterOrderId")]
        public string CounterOrderId { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("liquidity")]
        public string Liquidity { get; set; }

        [JsonProperty("forceTaker")]
        public bool ForceTaker { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("size")]
        public decimal Size { get; set; }

        [JsonProperty("funds")]
        public decimal Funds { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("feeRate")]
        public decimal FeeRate { get; set; }

        [JsonProperty("feeCurrency")]
        public string FeeCurrency { get; set; }

        [JsonProperty("stop")]
        public string Stop { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("createdAt")]
        public long CreatedAt { get; set; }

        [JsonProperty("tradeType")]
        public string TradeType { get; set; }
    }
}