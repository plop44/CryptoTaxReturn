using System;

namespace Common.Conversions
{
    public class FiatConversionWithMinDate : IFiatConversion
    {
        private readonly IFiatConversion _fiatConversion;
        private readonly long _taxStartingDate;

        public FiatConversionWithMinDate(IFiatConversion fiatConversion, IPortfolioAcrossTimeConfig config)
        {
            _fiatConversion = fiatConversion;
            _taxStartingDate = config.GetNewTaxCitizenStartDateAsLong();
        }

        public string Name => _fiatConversion.Name;

        public decimal GetPriceEstimationInFiat(string asset, long time)
        {
            return _fiatConversion.GetPriceEstimationInFiat(asset, GetDate(time));
        }

        public decimal? TryGetExactPriceInBtc(string asset, long time)
        {
            return _fiatConversion.TryGetExactPriceInBtc(asset, GetDate(time));
        }

        public decimal? TryGetExactPriceInFiat(string asset, long time)
        {
            return _fiatConversion.TryGetExactPriceInFiat(asset, GetDate(time));
        }

        public long LastDate => _fiatConversion.LastDate;

        private long GetDate(long time)
        {
            if (time < _taxStartingDate) return _taxStartingDate;
            return time;
        }
    }
}