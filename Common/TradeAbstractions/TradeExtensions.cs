using System;
using Common.Models;

namespace Common.TradeAbstractions
{
    public static class TradeExtensions
    {
        public static string GetSymbol(this ITrade trade) => trade.Base + trade.Quote;
        public static DateTime GetTimeReadable(this ITrade trade) => DateTimeOffset.FromUnixTimeMilliseconds(trade.Time).DateTime;
        public static decimal GetQuoteQuantity(this ITrade tradeWithRates)
        {
            return (tradeWithRates.Quantity * tradeWithRates.Price).CleanValueTo8Decimals();
        }
    }
}