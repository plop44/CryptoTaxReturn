using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class HistoSymbolPrice
    {
        [JsonProperty("Symbol")] public string Symbol { get; set; }
        [JsonProperty("Base")] public string Base { get; set; }
        [JsonProperty("Quote")] public string Quote { get; set; }
        [JsonProperty("Date")] public long Date { get; set; }
        [JsonProperty("Price")] public decimal Price { get; set; }
        public DateTime TimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(Date).DateTime;
    }
}