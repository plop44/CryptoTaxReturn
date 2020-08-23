using System;
using System.Linq;
using Common;
using Common.Conversions;
using Common.Files;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public class BalanceDifferencesPortfolioLogger
    {
        private readonly Portfolio _portfolioAcrossTime;
        private readonly LocalDataRetrieverComposite _dataRetriever;
        private readonly IFiatConversion _fiatConversion;
        private readonly CurrentFiat _currentFiat;
        private readonly ILogger _logger;

        public BalanceDifferencesPortfolioLogger(Portfolio portfolioAcrossTime,
            LocalDataRetrieverComposite dataRetriever,
            IFiatConversion fiatConversion, CurrentFiat currentFiat, LoggerFactory loggerFactory)
        {
            _portfolioAcrossTime = portfolioAcrossTime;
            _dataRetriever = dataRetriever;
            _fiatConversion = fiatConversion;
            _currentFiat = currentFiat;
            _logger = loggerFactory.GetLogger();
        }

        public void LogDifferences()
        {
            _logger.LogHeader("DIF");
            var snapshot = _portfolioAcrossTime.GetAssets();
            var snapshotBalances = snapshot.Where(t => t.Amount != 0).ToDictionary(t => t.Name);
            var balances = _dataRetriever.GetAccountInfoBalances().Where(t => t.GetTotal() != 0).ToDictionary(t => t.Asset);

            var totalDiff = 0m;
            foreach (var asset in snapshotBalances.Keys.Concat(balances.Keys).Distinct().OrderBy(t => t))
            {
                var calculatedValue = snapshotBalances.TryGetValue(asset, out var calc) ? calc.Amount : 0m;
                var actualValue = balances.TryGetValue(asset, out var actual) ? actual.GetTotal() : 0m;
                var diffAsset = Math.Abs(calculatedValue - actualValue);

                if (diffAsset == 0) continue;

                var diffFiat = _fiatConversion.GetPriceEstimationInFiat(asset, _fiatConversion.LastDate) * diffAsset;
                _logger.LogLine($"{asset}: calc {calculatedValue:N8}, actual {actualValue:n8}, diff {diffAsset:N8} => {diffFiat:N8} {_currentFiat.Name}");

                totalDiff += diffFiat;
            }

            _logger.LogLine($"TOTAL diff {totalDiff:N8} {_currentFiat.Name}");
        }
    }
}