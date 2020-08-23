using System;

namespace Common.Conversions
{
    public interface IFiatConversion
    {
        string Name { get; }
        decimal GetPriceEstimationInFiat(string asset, long time);
        decimal? TryGetExactPriceInBtc(string asset, long time);
        decimal? TryGetExactPriceInFiat(string asset, long time);
        long LastDate { get; }
    }
}