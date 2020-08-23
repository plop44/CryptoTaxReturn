using System;
using System.Linq;
using Common;

namespace CapitalGainGenerator.Loggers
{
    public class OffExchangeAssetPortfolioLogger
    {
        private readonly Portfolio _portfolioExact;
        private readonly ILogger _logger;

        public OffExchangeAssetPortfolioLogger(Portfolio portfolioExact, LoggerFactory loggerFactory)
        {
            _portfolioExact = portfolioExact;
            _logger = loggerFactory.GetLogger();
        }

        public void Log()
        {
            _logger.LogHeader("OFF EXCHANGE");
            var offExchangeSnapshot = _portfolioExact.GetOffExchangeSnapshot().Where(t => t.Amount != 0).OrderBy(t=>t.Name);

            foreach (var asset in offExchangeSnapshot)
            {
                _logger.LogLine($"{asset.Name} {asset.Amount}");
            }
        }
    }
}