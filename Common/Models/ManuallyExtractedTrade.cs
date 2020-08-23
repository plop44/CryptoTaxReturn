using System;

namespace Common.Models
{
    public sealed class ManuallyExtractedTrade
    {
        public ManuallyExtractedTrade(string value)
        {
            var strings = value.Split(",");
            TimeReadable = DateTime.Parse(strings[0]);
            Symbol = strings[1];
            IsBuyer = strings[2] == "BUY";
            Price = decimal.Parse(strings[3]);
            Quantity = decimal.Parse(strings[4]);
            Commission = decimal.Parse(strings[6]);
            CommissionAsset = strings[7];

            if (strings.Length >= 9)
            {
                var previous = TimeReadable;
                // we got a fixed time to fix breached
                // fix would work but is too long to do manually
                TimeReadable = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(strings[8])).DateTime;

                if(Math.Abs((previous - TimeReadable).TotalMinutes) >1)
                    throw new Exception("manual fix is wrong");
            }
        }

        public long Time => TimeReadable.ToUnixDateTime();
        public DateTime TimeReadable { get; }
        public string Symbol { get; }
        public bool IsBuyer { get; }
        public decimal Price { get; }
        public decimal Quantity { get; }
        public decimal Commission { get; }
        public string CommissionAsset { get; }
    }
}