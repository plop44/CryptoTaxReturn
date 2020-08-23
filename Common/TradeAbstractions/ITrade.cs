namespace Common.TradeAbstractions
{
    public interface ITrade
    {
        long Time { get; }
        string Base { get; }
        string Quote { get; }
        bool IsBuyer { get; }
        decimal Quantity { get; }
        decimal Price { get; }
        string CommissionAsset { get; }
        decimal Commission { get;  }
    }
}