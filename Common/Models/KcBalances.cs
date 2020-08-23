using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class KcBalances
    {
        [JsonProperty("MyArray")]
        public List<KcBalance> MyArray { get; set; }
    }
}