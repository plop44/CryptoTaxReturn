using Newtonsoft.Json;

namespace Extract.KuCoin
{
#nullable disable
    public class ApiResponse<T>
    {
        [JsonProperty(PropertyName = "code")] public string Code { get; set; }
        [JsonProperty(PropertyName = "data")] public T Data { get; set; }
    }
}