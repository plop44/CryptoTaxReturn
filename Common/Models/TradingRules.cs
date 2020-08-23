using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class TradingRules
    {
        [JsonProperty("timezone")]
        public string Timezone { get; set; }
        [JsonProperty("serverTime")]
        public long ServerTime { get; set; }
        [JsonProperty("rateLimits")]
        public IEnumerable<RateLimit> RateLimits { get; set; }
        [JsonProperty("symbols")]
        public List<Symbol> Symbols { get; set; }
    }
}
