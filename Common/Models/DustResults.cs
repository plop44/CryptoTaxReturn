using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class DustResults
    {
        [JsonProperty("rows")] public DustRow[] Rows { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
    }
}