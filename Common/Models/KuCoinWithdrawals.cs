using System.Collections.Generic;
using Newtonsoft.Json;

public class KuCoinWithdrawals
{
#nullable disable
    [JsonProperty("currentPage")]
    public int CurrentPage { get; set; }

    [JsonProperty("pageSize")]
    public int PageSize { get; set; }

    [JsonProperty("totalNum")]
    public int TotalNum { get; set; }

    [JsonProperty("totalPage")]
    public int TotalPage { get; set; }

    [JsonProperty("items")]
    public List<KuCoinWithdrawal> Items { get; set; }
}