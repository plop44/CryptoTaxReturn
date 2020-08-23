using System;
using System.Linq;
using Common.Conversions;
using Common.Files;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public class InputOutputLogger
    {
        private readonly IFiatConversion _fiatConversion;
        private readonly CurrentFiat _currentFiat;
        private readonly LocalDataRetrieverComposite _dataRetriever;
        private readonly ILogger _logger;

        public InputOutputLogger(IFiatConversion fiatConversion, CurrentFiat currentFiat,
            LocalDataRetrieverComposite dataRetriever, LoggerFactory loggerFactory)
        {
            _fiatConversion = fiatConversion;
            _currentFiat = currentFiat;
            _dataRetriever = dataRetriever;
            _logger = loggerFactory.GetLogger();
        }

        public void Log()
        {
            _logger.LogHeader("Input/Output analysis");
            var deposit = _dataRetriever.GetDeposits().Sum(t => _fiatConversion.GetPriceEstimationInFiat(t.Asset, t.InsertTime) * t.Amount);
            var withdraws = _dataRetriever.GetWithdraws().Sum(t => _fiatConversion.GetPriceEstimationInFiat(t.Asset, t.ApplyTime) * t.Amount);
            var balancesValue = _dataRetriever.GetAccountInfoBalances().Where(t=>t.GetTotal()!=0).Sum(t => _fiatConversion.GetPriceEstimationInFiat(t.Asset, _fiatConversion.LastDate) * t.GetTotal());
            _logger.LogLine($"Deposit {deposit:N0} {_currentFiat.Name}");
            _logger.LogLine($"Withdraws {withdraws:N0} {_currentFiat.Name}");
            _logger.LogLine($"Balances {balancesValue:N0} {_currentFiat.Name}");
            _logger.LogLine($"balances + withdraws - deposit = {balancesValue + withdraws - deposit:N0} {_currentFiat.Name}");
        }
    }
}