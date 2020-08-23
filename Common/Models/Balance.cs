using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class Balance
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }
        [JsonProperty("free")]
        public decimal Free { get; set; }
        [JsonProperty("locked")]
        public decimal Locked { get; set; }
    }
}