using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class TickerStatistics
    {
        [JsonProperty("symbol")] public string Symbol { get; set; }//"symbol": "BNBBTC",
        [JsonProperty("priceChange")] public double PriceChange { get;set;}//"priceChange": "-94.99999800",
        [JsonProperty("priceChangePercent")] public double PriceChangePercent { get; set; }//"priceChangePercent": "-95.960",
        [JsonProperty("weightedAvgPrice")] public double WeightedAvgPrice { get; set; }//"weightedAvgPrice": "0.29628482",
        [JsonProperty("prevClosePrice")] public double PrevClosePrice { get; set; }//"prevClosePrice": "0.10002000",
        [JsonProperty("lastPrice")] public double LastPrice { get; set; }//"lastPrice": "4.00000200",
        [JsonProperty("lastQty")] public double LastQty { get; set; }//"lastQty": "200.00000000",
        [JsonProperty("bidPrice")] public double BidPrice { get; set; }//"bidPrice": "4.00000000",
        [JsonProperty("askPrice")] public double AskPrice { get; set; }//"askPrice": "4.00000200",
        [JsonProperty("openPrice")] public double OpenPrice { get; set; }//"openPrice": "99.00000000",
        [JsonProperty("highPrice")] public double HighPrice { get; set; }//"highPrice": "100.00000000",
        [JsonProperty("lowPrice")] public double LowPrice { get; set; }//"lowPrice": "0.10000000",
        [JsonProperty("volume")] public double Volume { get; set; }//"volume": "8913.30000000",
        [JsonProperty("quoteVolume")] public double QuoteVolume { get; set; }//"quoteVolume": "15.30000000",
        [JsonProperty("openTime")] public long OpenTime { get; set; }//"openTime": 1499783499040,
        [JsonProperty("closeTime")] public long CloseTime { get; set; }//"closeTime": 1499869899040,
        [JsonProperty("firstId")] public int FirstId { get; set; }//"firstId": 28385,   // First tradeId
        [JsonProperty("lastId")] public int LastId { get; set; }//"lastId": 28460,    // Last tradeId
        [JsonProperty("count")] public int Count { get; set; }//"count": 76 // Trade count	
    }
}