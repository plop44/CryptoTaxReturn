using System;
using Common.Delisting;
using Common.Launchpad;
using Common.Models;
using Common.TradeAbstractions;

namespace Common.Conversions
{
    public static class FiatConversionExtensions
    {
        public static decimal GetProceed(this IFiatConversion fiatConversion, ITrade trade)
        {
            if (trade is LaunchpadTrade)
            {
                // we go through BNB
                var bnbValue = fiatConversion.TryGetExactPriceInFiat(trade.Quote, trade.Time);
                if(bnbValue.HasValue)
                    return trade.GetQuoteQuantity() * bnbValue.Value;
            }

            if (trade.IsBuyer)
            {
                // we are buying baseCcy
                var result = fiatConversion.TryGetExactPriceInFiat(trade.Base, trade.Time);
                if (result.HasValue)
                {
                    return trade.Quantity * result.Value;
                }
            }

            var tryGetExactPriceInFiat = fiatConversion.TryGetExactPriceInFiat(trade.Quote, trade.Time);

            if (tryGetExactPriceInFiat.HasValue)
                return trade.GetQuoteQuantity() * tryGetExactPriceInFiat.Value;

            if (trade is DelistingTrade)
            {
                // for delisting trade we are trying up to 7 days after
                for (int i = 1; i <= 7; i++)
                {
                    var resultViaBase = fiatConversion.TryGetExactPriceInFiat(trade.Base, trade.Time.AddDays(i));
                    if (resultViaBase.HasValue)
                    {
                        return trade.Quantity * resultViaBase.Value;
                    }

                    var resultViaQuote = fiatConversion.TryGetExactPriceInFiat(trade.Quote, trade.Time.AddDays(i));

                    if (resultViaQuote.HasValue)
                        return trade.GetQuoteQuantity() * resultViaQuote.Value;

                    var baseViaBtc = fiatConversion.TryGetExactPriceInBtc(trade.Base, trade.Time.AddDays(i));
                    if (baseViaBtc.HasValue)
                        return trade.Quantity * baseViaBtc.Value * fiatConversion.GetExactPriceInFiat("BTC", trade.Time.AddDays(i));

                    var quoteViaBtc = fiatConversion.TryGetExactPriceInBtc(trade.Quote, trade.Time.AddDays(i));
                    if (quoteViaBtc.HasValue)
                        return trade.GetQuoteQuantity() * quoteViaBtc.Value * fiatConversion.GetExactPriceInFiat("BTC", trade.Time.AddDays(i));
                }
            }

            throw new Exception($"Unable to price {trade.GetSymbol()} for date {trade.Time} {trade.GetTimeReadable():dd/MM/yyyy}");
        }

        public static decimal GetExactPriceInFiatWithBtcFallbackEnable(this IFiatConversion fiatConversion, string asset, long time)
        {
            var exactPrice = fiatConversion.TryGetExactPriceInFiat(asset, time);

            if (exactPrice.HasValue)
                return exactPrice.Value;

            var exactBtcPrice = fiatConversion.GetExactPriceInFiat("BTC", time);
            var exactAssetPriceInBtc = fiatConversion.GetExactPriceInBtc(asset, time);

            return exactBtcPrice * exactAssetPriceInBtc;
        }

        public static decimal GetExactPriceInFiat(this IFiatConversion fiatConversion, string asset, long time)
        {
            var tryGetExactPriceInFiat = fiatConversion.TryGetExactPriceInFiat(asset,time);

            if(!tryGetExactPriceInFiat.HasValue)
                throw new Exception($"No exact {fiatConversion.Name} price can be given for {asset} @{time.ToTimeReadable():g} - {time}");

            return tryGetExactPriceInFiat.Value;
        }

        public static decimal GetExactPriceInBtc(this IFiatConversion fiatConversion, string asset, long time)
        {
            var tryGetExactPriceInBtc = fiatConversion.TryGetExactPriceInBtc(asset,time);

            if(!tryGetExactPriceInBtc.HasValue)
                throw new Exception($"No exact BTC price can be given for {asset} @{time.ToTimeReadable():g} - {time}");

            return tryGetExactPriceInBtc.Value;
        }
    }
}
