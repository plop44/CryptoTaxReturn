﻿using System;
using Newtonsoft.Json;

namespace AddingRates
{
    public class TradeWithRates
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("qty")]
        public decimal Quantity { get; set; }
        [JsonProperty("commission")]
        public decimal Commission { get; set; }
        [JsonProperty("commissionAsset")]
        public string CommissionAsset { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("isBuyer")]
        public bool IsBuyer { get; set; }
        [JsonProperty("isMaker")]
        public bool IsMaker { get; set; }

        [JsonProperty("isBestMatch")]
        public bool IsBestMatch { get; set; }

        [JsonProperty("QuoteToUsdtRate")]
        public decimal QuoteToUsdtRate { get; set; }

        [JsonProperty("CommissionAssetToUsdtRate")]
        public decimal CommissionAssetToUsdtRate { get; set; }

        [JsonProperty("AudToUsdRate")]
        public decimal AudToUsdRate { get; set; }
        public DateTime TimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(Time).DateTime;
    }
}