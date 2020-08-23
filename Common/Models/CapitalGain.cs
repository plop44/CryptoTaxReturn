using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class CapitalGain
    {
        [JsonProperty("boughtTime")] public long BoughtTime { get; set; }
        [JsonProperty("soldTime")] public long SoldTime { get; set; }
        [JsonProperty("asset")] public string Asset { get; set; }
        [JsonProperty("qty")] public decimal Quantity { get; set; }
        [JsonProperty("proceed")] public decimal Proceed { get; set; }
        [JsonProperty("cost")] public decimal Cost { get; set; }
        [JsonProperty("boughtTag")] public string BoughtTag { get; set; }
        public decimal Gain => Proceed - Cost;
        public DateTime BoughtTimeReadable => BoughtTime.ToTimeReadable();
        public DateTime SoldTimeReadable => SoldTime.ToTimeReadable();

        public void CleanValues()
        {
            Quantity = Quantity.CleanValueTo8Decimals();
            Proceed = Proceed.CleanValueTo8Decimals();
            Cost = Cost.CleanValueTo8Decimals();
        }
    }
}