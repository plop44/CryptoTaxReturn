using System;
using System.Collections.Immutable;
using System.Linq;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public sealed class CapitalGainPerAssetLogger
    {
        private readonly CurrentFiat _currentFiat;
        private readonly ILogger _logger;

        public CapitalGainPerAssetLogger(CurrentFiat currentFiat, LoggerFactory loggerFactory)
        {
            _currentFiat = currentFiat;
            _logger = loggerFactory.GetLogger();
        }

        public void Log(ImmutableArray<CapitalGain> capitalGains)
        {
            _logger.LogHeader("Capital gain per asset");

            var gainPerAsset = capitalGains.GroupBy(t => t.Asset)
                .Select(t => (t.Key, t.Sum(t2 => t2.Gain), t.Sum(t2 => t2.Proceed)))
                .OrderByDescending(t => t.Item2);

            foreach (var asset in gainPerAsset)
            {
                _logger.LogLine($"Asset {asset.Key} {asset.Item2:N2} {_currentFiat.Name}, proceeds {asset.Item3:N0}");
            }
        }
    }
}