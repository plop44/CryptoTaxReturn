using System;
using System.Globalization;
using Common;
using Common.Conversions;
using Common.Files;
using Common.Models;
using Common.Rest;
using Newtonsoft.Json;

namespace CapitalGainGenerator
{
    public class Config : ICurrentFiatConfig, ILocalDataRetrieverCompositeConfig, IPortfolioAcrossTimeConfig, ITaxMethodologyConfig
    {
        [JsonProperty("startOfFinancialYear")] public string? StartOfFinancialYear { get; set; }
        [JsonProperty("currency")] public string Currency { get; set; } = "USD";
        [JsonProperty("methodology")] public TaxMethodology TaxMethodology { get; set; } = TaxMethodology.Hifo;
        [JsonProperty("newTaxCitizenStartDate")] public string? NewTaxCitizenStartDate { get; set; }
        [JsonProperty("accounts")] public string Account { get; set; } = string.Empty;
        public bool IsNewTaxCitizenStartDateApplies => NewTaxCitizenStartDate != null;
        public long GetNewTaxCitizenStartDateAsLong() => DateTime.ParseExact(NewTaxCitizenStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture).ToUnixDateTime();
    }
}