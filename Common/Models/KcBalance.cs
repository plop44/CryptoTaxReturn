using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class KcBalance
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("available")]
        public decimal Available { get; set; }

        [JsonProperty("holds")]
        public decimal Holds { get; set; }
    }
}