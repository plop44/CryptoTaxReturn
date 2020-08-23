using System;

namespace Common.Models
{
#nullable disable
    public class Dividend
    {
        public decimal Amount { get; set; }
        public string Asset { get; set; }
        public long DivTime { get; set; }
        public string EnInfo { get; set; }
        public long TranId { get; set; }
        public DateTime TimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(DivTime).DateTime;
    }
}