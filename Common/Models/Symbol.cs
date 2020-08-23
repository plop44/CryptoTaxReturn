﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class Symbol
    {
        [JsonProperty("symbol")]
        public string SymbolName { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("baseAsset")]
        public string BaseAsset { get; set; }
        [JsonProperty("baseAssetPrecision")]
        public int BaseAssetPrecision { get; set; }
        [JsonProperty("quoteAsset")]
        public string QuoteAsset { get; set; }
        [JsonProperty("quotePrecision")]
        public int QuotePrecision { get; set; }
        [JsonProperty("orderTypes")]
        public IEnumerable<string> OrderTypes { get; set; }
        [JsonProperty("icebergAllowed")]
        public bool IcebergAllowed { get; set; }
        [JsonProperty("filters")]
        public IEnumerable<Filter> Filters { get; set; }
    }
}
