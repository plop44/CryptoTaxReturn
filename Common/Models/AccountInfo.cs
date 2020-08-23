using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class AccountInfo
    {
        [JsonProperty("makerCommission")]
        public int MakerCommission { get; set; }
        [JsonProperty("takerCommission")]
        public int TakerCommission { get; set; }
        [JsonProperty("buyerCommission")]
        public int BuyerCommission { get; set; }
        [JsonProperty("sellerCommission")]
        public int SellerCommission { get; set; }
        [JsonProperty("canTrade")]
        public bool CanTrade { get; set; }
        [JsonProperty("canWithdraw")]
        public bool CanWithdraw { get; set; }
        [JsonProperty("canDeposit")]
        public bool CanDeposit { get; set; }
        [JsonProperty("balances")]
        public IEnumerable<Balance> Balances { get; set; }
    }
}
