using System;
using Common.Models;

namespace Common.Conversions
{
    public class AudFiatConversion : IFiatConversion
    {
        private readonly UsdFiatConversion _usdFiatConversion;

        private readonly AudUsdFxRate _audUsdFxRate;

        public AudFiatConversion(UsdFiatConversion usdFiatConversion, AudUsdFxRate audUsdFxRate)
        {
            _usdFiatConversion = usdFiatConversion;
            _audUsdFxRate = audUsdFxRate;

            LastDate = Math.Min(usdFiatConversion.LastDate, _audUsdFxRate.LastDate);
        }

        public string Name => "AUD";

        public decimal GetPriceEstimationInFiat(string asset, long time)
        {
            return _usdFiatConversion.GetPriceEstimationInFiat(asset, time)
                   * _audUsdFxRate.GetMultiplierRateFromUsdToAudAsAnEstimation(time);
        }

        public decimal? TryGetExactPriceInBtc(string asset, long time)
        {
            return _usdFiatConversion.TryGetExactPriceInBtc(asset, time);
        }

        public decimal? TryGetExactPriceInFiat(string asset, long time)
        {
            var tryGetExactPriceInFiat = _usdFiatConversion.TryGetExactPriceInFiat(asset, time);

            if (!tryGetExactPriceInFiat.HasValue)
                return null;

            return tryGetExactPriceInFiat.Value * _audUsdFxRate.GetMultiplierRateFromUsdToAud(time);
        }

        public long LastDate { get; }
    }
}