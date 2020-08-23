using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Common;
using Common.Files;
using Common.Models;
using Common.Rest;
using Newtonsoft.Json;

namespace CapitalGainGenerator.Loggers
{
    public sealed class TaxReportLogger
    {
        private readonly Config _config;
        private readonly CurrentFiat _currentFiat;
        private readonly CapitalGainReportSaver _capitalGainReportSaver;
        private readonly LocalDataRetrieverComposite _localDataRetrieverComposite;
        private readonly ILogger _logger;
        private static readonly long OneYearInMilliseconds = (long) TimeSpan.FromDays(365).TotalMilliseconds;

        public TaxReportLogger(Config config, CurrentFiat currentFiat, CapitalGainReportSaver capitalGainReportSaver, LoggerFactory loggerFactory, LocalDataRetrieverComposite localDataRetrieverComposite)
        {
            _config = config;
            _currentFiat = currentFiat;
            _capitalGainReportSaver = capitalGainReportSaver;
            _localDataRetrieverComposite = localDataRetrieverComposite;
            _logger = loggerFactory.GetLogger();
        }

        public decimal Log(ImmutableArray<CapitalGain> capitalGains)
        {
            if (capitalGains.Length == 0)
                return 0;

            _logger.LogHeader("Capital gain per year");
            _logger.LogLine($"Account(s): {_localDataRetrieverComposite.Account}\n");
            _logger.LogLine("S/T: short-term, asset has been held for LESS than one year");
            _logger.LogLine("L/T long-term, asset has been held for MORE than one year");

            var capitalGain = capitalGains[0];

            var capitalGainBoughtTimeReadable = capitalGain.BoughtTimeReadable;

            var startTime = new DateTime(capitalGainBoughtTimeReadable.Year,
                int.Parse(_config.StartOfFinancialYear.Split('/')[1]),
                int.Parse(_config.StartOfFinancialYear.Split('/')[0]),
                0, 0, 0);

            if (startTime > capitalGainBoughtTimeReadable)
                startTime = startTime.AddYears(-1);

            var endTime = startTime.AddYears(1);

            decimal runningResult = 0;
            decimal runningResultLowTax = 0;
            decimal total = 0;
            decimal totalLowTax = 0;
            foreach (var gain in capitalGains)

            {
                if (gain.SoldTimeReadable > endTime)
                {
                    _logger.LogLine(
                        $"{endTime.AddYears(-1):dd/MM/yyyy} - {endTime.AddDays(-1):dd/MM/yyyy}: S/T {runningResult:N2} {_currentFiat.Name}, L/T {runningResultLowTax:N2} {_currentFiat.Name}");
                    endTime = endTime.AddYears(1);
                    runningResult = 0;
                    runningResultLowTax = 0;
                }

                if (gain.SoldTime - gain.BoughtTime < OneYearInMilliseconds || gain.Gain < 0)
                {
                    runningResult += gain.Gain;
                    total += gain.Gain;
                }
                else
                {
                    runningResultLowTax += gain.Gain;
                    totalLowTax += gain.Gain;
                }
            }

            _logger.LogLine(
                $"{endTime.AddYears(-1):dd/MM/yyyy} - {endTime.AddDays(-1):dd/MM/yyyy}: S/T {runningResult:N2} {_currentFiat.Name}, L/T {runningResultLowTax:N2} {_currentFiat.Name}");
            _logger.LogLine($"Total capital gain: S/T {total:N2} {_currentFiat.Name}, L/T {totalLowTax:N2} {_currentFiat.Name}");
            _logger.LogLine($"Total: {total + totalLowTax:N2} {_currentFiat.Name}");

            var lines = capitalGains.Do(t => t.CleanValues()).Select(JsonConvert.SerializeObject);
            File.WriteAllLines(Path.Combine(_capitalGainReportSaver.SavingFolder, "CapitalGain.json"), lines);

            return total + totalLowTax;
        }

        public void LogLine(string line) => _logger.LogLine(line);
    }
}