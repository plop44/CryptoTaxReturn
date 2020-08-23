using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Models;
using Common.TradeAbstractions;

namespace Common.Launchpad
{
    public sealed class LaunchpadTradeGenerator
    {
        private readonly Dictionary<string, LaunchpadEntry> _entries;
        private static readonly double TotalMilliseconds = TimeSpan.FromDays(1).TotalMilliseconds;

        public LaunchpadTradeGenerator()
        {
            _entries = File.ReadAllLines("./YourHomework/Launchpad.csv").Skip(1)
                .Where(t=>!t.StartsWith("//"))
                .Select(t => new LaunchpadEntry(t))
                .ToDictionary(t => t.Asset);
        }

        public ITrade? TryGetTrade(Dividend dividend)
        {
            if (_entries.TryGetValue(dividend.Asset, out var entry))
            {
                var entryDividendTimeApproximation = Math.Abs(dividend.DivTime - entry.DividendTimeApproximation);
                if (entryDividendTimeApproximation < TotalMilliseconds)
                {
                    return new LaunchpadTrade(dividend.DivTime,dividend.Asset,dividend.Amount, entry.Price);
                }
            }

            return null;
        }
    }
}