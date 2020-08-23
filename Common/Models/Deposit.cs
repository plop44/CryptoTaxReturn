using System;
using Newtonsoft.Json;

namespace Common.Models
{
#nullable disable
    public class Deposit
    {            
        //"insertTime": 1508198532000,
        //"amount": 0.04670582,
        //"asset": "ETH",
        //"address": "0x6915f16f8791d0a1cc2bf47c13a6b2a92000504b",
        //"txId": "0xdf33b22bdb2b28b1f75ccd201a4a4m6e7g83jy5fc5d5a9d1340961598cfcb0a1",
        //"status": 1
        [JsonProperty("insertTime")]public long InsertTime { get; set; }
        [JsonProperty("asset")]public string Asset { get; set; }
        [JsonProperty("amount")]public decimal Amount { get; set; }
        [JsonProperty("address")]public string Address { get; set; }
        [JsonProperty("txId")]public string TransactionId { get; set; }
        [JsonProperty("status")]public int Status { get; set; }
        public DateTime TimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(InsertTime).DateTime;
    }
}