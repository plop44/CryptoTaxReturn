using System;
using System.IO;
using Common;
using Common.Files;
using Common.Rest;

namespace CapitalGainGenerator
{
    public class CapitalGainReportSaver
    {
        public CapitalGainReportSaver(TaxMethodology taxMethodology, CurrentFiat currentFiat, PathsConfig pathsConfig)
        {
            SavingFolder = AccountFileRepository.GetFolder(Path.Combine(pathsConfig.ReportGenerationFolder, $"{currentFiat.Name}_{taxMethodology}_{DateTime.Today:yyyyMMdd}"));
            File.Copy("Config.json", Path.Combine(SavingFolder, "Config.json"));
        }

        public string SavingFolder { get;  } 
    }
}