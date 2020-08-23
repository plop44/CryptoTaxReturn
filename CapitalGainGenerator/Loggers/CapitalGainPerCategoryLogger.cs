using System.Collections.Immutable;
using System.Linq;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public sealed class CapitalGainPerCategoryLogger
    {
        private readonly CurrentFiat _currentFiat;
        private readonly ILogger _logger;

        public CapitalGainPerCategoryLogger(CurrentFiat currentFiat, LoggerFactory loggerFactory)
        {
            _currentFiat = currentFiat;
            _logger = loggerFactory.GetLogger();
        }

        public void Log(ImmutableArray<CapitalGain> capitalGains)
        {
            _logger.LogHeader("Capital gain per tag");
            var gainPerTag = capitalGains.GroupBy(t => t.BoughtTag).Select(t => (t.Key, t.Sum(t2 => t2.Gain), t.Count()));

            foreach (var (key, value, count) in gainPerTag)
            {
                var tagKey = string.IsNullOrEmpty(key)?"Empty":key;
                _logger.LogLine($"Category {tagKey} {value:N2} {_currentFiat.Name}, count {count:N0}");
            }
        }
    }
}