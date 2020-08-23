using System;
using System.Linq;
using Common;
using Common.Conversions;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public sealed class EndOfFinancialYearPortfolioLogger
    {
        private readonly Portfolio _portfolio;
        private readonly DateTime _financialYearStart;
        private DateTime? _previous;
        private readonly ILogger _logger;
        private readonly IFiatConversion _fiatConversion;
        private readonly CurrentFiat _currentFiat;

        public EndOfFinancialYearPortfolioLogger(PortfolioAcrossTime portfolioAcrossTime, 
            Portfolio portfolio, Config config,LoggerFactory loggerFactory, IFiatConversion fiatConversion, CurrentFiat currentFiat)
        {
            _portfolio = portfolio;
            _fiatConversion = fiatConversion;
            _currentFiat = currentFiat;
            portfolioAcrossTime.BeforeProcessing += OnBeforeProcessing;

            _financialYearStart = new DateTime(1,
                int.Parse(config.StartOfFinancialYear.Split('/')[1]),
                int.Parse(config.StartOfFinancialYear.Split('/')[0]),
                0, 0, 0);

            _logger = loggerFactory.GetLogger();
        }

        private void OnBeforeProcessing(long current)
        {
            var currentAsDateTime = current.ToTimeReadable();

            var nextFinancialYear = _financialYearStart.AddYears(currentAsDateTime.Year - 1);

            if (currentAsDateTime >= nextFinancialYear && _previous.HasValue && _previous < nextFinancialYear)
            {
                var assets = _portfolio.GetAssets().Concat(_portfolio.GetOffExchangeSnapshot()).GroupBy(t => t.Name).Select(t => (t.Key, t.Sum(t2 => t2.Amount)))
                    .Where(t => t.Item2 != 0).OrderBy(t => t.Key);

                _logger.LogHeader($"Portfolio as of {nextFinancialYear:dd/MM/yyyy}");
                decimal totalInFiat = 0;
                foreach (var asset in assets)
                {
                    var priceEstimationInFiat = asset.Item2 * _fiatConversion.GetPriceEstimationInFiat(asset.Key, nextFinancialYear.ToUnixDateTime());
                    totalInFiat += priceEstimationInFiat;
                    _logger.LogLine($"{asset.Key} {asset.Item2:N8} in asset, {priceEstimationInFiat:N2} {_currentFiat.Name}");
                }

                _logger.LogLine($"Total {totalInFiat:N2} {_currentFiat.Name}\n");
            }

            _previous = currentAsDateTime;
        }
    }
}