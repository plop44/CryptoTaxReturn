using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Models;
using Common.TradeAbstractions;

namespace Common.Delisting
{
    public sealed class DelistingTradeGenerator
    {
        private readonly Portfolio _portfolio;
        private readonly Dictionary<long, DelistingEntry> _entries;

        public DelistingTradeGenerator(Portfolio portfolio)
        {
            _portfolio = portfolio;
            _entries = File.ReadAllLines("./YourHomework/Delisting.csv")
                .Skip(1)
                .Where(t=>!t.StartsWith("//"))
                .Select(t => new DelistingEntry(t))
                .GroupBy(t=>t.Time)
                .Select(t=>t.First())
                .ToDictionary(t => t.Time);
        }

        public ITrade? TryGetTrade(Dividend dividend)
        {
            if (_entries.TryGetValue(dividend.DivTime, out var entry))
            {
                if (dividend.Asset == entry.Quote)
                {
                    // some BCC=>BCHSV && BCHSV=>BCHABC should be counted once only
                    var actualAssetValue = _portfolio.GetAsset(entry.Base).Amount;

                    if (actualAssetValue >= (dividend.Amount / entry.Price).CleanValueTo8Decimals())
                        return new DelistingTrade(entry, dividend, actualAssetValue);
                }
            }

            return null;
        }
    }
}