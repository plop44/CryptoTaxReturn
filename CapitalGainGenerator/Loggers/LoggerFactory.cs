using System.IO;
using CapitalGainGenerator.Loggers;

namespace CapitalGainGenerator
{
    public sealed class LoggerFactory
    {
        private readonly CapitalGainReportSaver _capitalGainReportSaver;

        public LoggerFactory(CapitalGainReportSaver capitalGainReportSaver)
        {
            _capitalGainReportSaver = capitalGainReportSaver;
        }

        public ILogger GetLogger([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath).Replace("Logger",string.Empty);
            var combine = Path.Combine(_capitalGainReportSaver.SavingFolder,fileNameWithoutExtension+".txt");

            if(sourceFilePath.Contains(nameof(EndOfFinancialYearPortfolioLogger))||sourceFilePath.Contains(nameof(UnrealizedCapitalGainLogger))||sourceFilePath.Contains(nameof(CapitalGainPerAssetLogger)))
                return new FileLogger(combine);

            return new ConsoleAndFileLogger(combine);
        }
    }
}