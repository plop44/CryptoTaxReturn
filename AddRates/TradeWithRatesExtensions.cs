using Common.Model;

namespace AddingRates
{
    public static class TradeWithRatesExtensions
    {
        public static decimal GetCommissionInAud(this TradeWithRates tradeWithRates)
        {
            return tradeWithRates.Commission * tradeWithRates.CommissionAssetToUsdtRate;
        }
        public static decimal GetBaseOrQuoteValueInAud(this TradeWithRates tradeWithRates)
        {
            return tradeWithRates.Quantity * tradeWithRates.Price * tradeWithRates.QuoteToUsdtRate / tradeWithRates.AudToUsdRate;
        }
        public static decimal GetQuantityQuote(this TradeWithRates tradeWithRates)
        {
            return tradeWithRates.Quantity * tradeWithRates.Price;
        }
        public static decimal GetQuantityQuote(this Trade tradeWithRates)
        {
            return tradeWithRates.Quantity * tradeWithRates.Price;
        }
    }
}