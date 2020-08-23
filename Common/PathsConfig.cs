using Newtonsoft.Json;

namespace Common
{
    public class PathsConfig
    {
        [JsonProperty("apiKeyConfigPath")] public string ApiKeyConfigPath { get; set; } = string.Empty;
        [JsonProperty("extractFolder")] public string ExtractFolder { get; set; } = string.Empty;
        [JsonProperty("reportGenerationFolder")] public string ReportGenerationFolder { get; set; } = string.Empty;
    }
}