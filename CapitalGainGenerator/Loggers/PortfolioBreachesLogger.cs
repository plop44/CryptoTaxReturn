using System;
using System.Linq;
using Common;
using Common.Conversions;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public class PortfolioBreachesLogger
    {
        private readonly Portfolio _portfolioExact;
        private readonly IFiatConversion _fiatConversion;
        private readonly CurrentFiat _currentFiat;
        private readonly ILogger _logger;

        public PortfolioBreachesLogger(Portfolio portfolioExact, 
            IFiatConversion fiatConversion, CurrentFiat currentFiat,
            LoggerFactory loggerFactory)
        {
            _portfolioExact = portfolioExact;
            _fiatConversion = fiatConversion;
            _currentFiat = currentFiat;
            _logger = loggerFactory.GetLogger();
        }

        public void Log()
        {
            _logger.LogHeader("Breaches");
            var snapshots = _portfolioExact.GetAssets().Where(t => t.HasBreached)
                .OrderBy(t => t.Name).ToArray();

            if (snapshots.Length == 0)
            {
                _logger.LogLine("No Breaches, you got your tax return ready!");
                return;
            }

            _logger.LogLine("NegAmt: the balance for an asset has been negative. Some trades might be missing.");
            _logger.LogLine("ZeroCost: some cost are missing, a cost of 0 will be taken.");

            foreach (var asset in snapshots)
            {
                var assetBreachedAmount = asset.BreachedAmount * _fiatConversion.GetPriceEstimationInFiat(asset.Name, _fiatConversion.LastDate);
                _logger.LogLine($"{asset.Name} {asset.BreachCount} NegAmt breaches, {asset.SellSideBreaches} ZeroCost breaches for {assetBreachedAmount} {_currentFiat.Name}");
            }

            var breachLogs = snapshots.SelectMany(t => t.GetBreachLogs).ToArray();

            if(breachLogs.Length!=0)
                _logger.LogLine("\bBreach logs:");

            foreach (var breachLog in breachLogs)
            {
                _logger.LogLine(breachLog);
            }
        }
    }
}