using Common.TradeAbstractions;

namespace Common.Launchpad
{
    public class LaunchpadTrade : ITrade
    {
        public LaunchpadTrade(long time, string @base, decimal quantity, decimal price)
        {
            Time = time;
            Base = @base;
            Quantity = quantity;
            Price = price;
        }

        public long Time { get; }
        public string Base { get; }
        public string Quote => "BNB";
        public bool IsBuyer => true;
        public decimal Quantity { get; }
        public decimal Price { get; }
        public string CommissionAsset => "BNB";
        public decimal Commission => 0;
    }
}