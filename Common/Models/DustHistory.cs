using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class DustHistory
    {
        [JsonProperty("success")] public bool Success { get; set; }
        [JsonProperty("results")] public DustResults Results { get; set; }
    }
}