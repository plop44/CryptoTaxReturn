using Newtonsoft.Json;

namespace MyTradesExtract
{
#nullable disable
    public class Filter
    {
        [JsonProperty("filterType")]
        public string FilterType { get; set; }
        [JsonProperty("minPrice")]
        public double MinPrice { get; set; }
        [JsonProperty("maxPrice")]
        public double MaxPrice { get; set; }
        [JsonProperty("tickSize")]
        public double TickSize { get; set; }
        [JsonProperty("minQty")]
        public double MinQty { get; set; }
        [JsonProperty("maxQty")]
        public double MaxQty { get; set; }
        [JsonProperty("stepSize")]
        public double StepSize { get; set; }
        [JsonProperty("minNotional")]
        public double MinNotional { get; set; }
    }
}
