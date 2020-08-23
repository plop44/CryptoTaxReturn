using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class Position
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("value")]
        public decimal Value { get; set; }
    }
}