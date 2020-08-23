using System;
using System.Linq;
using Common;
using Common.Conversions;
using Common.Models;
using Common.Rest;

namespace CapitalGainGenerator.Loggers
{
    public class UnrealizedCapitalGainLogger
    {
        private readonly Portfolio _portfolioExact;
        private readonly IFiatConversion _fiatConversion;
        private readonly CurrentFiat _currentFiat;
        private readonly ILogger _logger;

        public UnrealizedCapitalGainLogger(Portfolio portfolioExact,
            IFiatConversion fiatConversion, CurrentFiat currentFiat, 
            LoggerFactory loggerFactory)
        {
            _portfolioExact = portfolioExact;
            _fiatConversion = fiatConversion;
            _currentFiat = currentFiat;
            _logger = loggerFactory.GetLogger();
        }
        
        public decimal Log()
        {
            _logger.LogHeader($"Unrealized capital gain as of {_fiatConversion.LastDate.ToTimeReadable():dd/MM/yyyy}");

            var assets = _portfolioExact.GetAssets()
                .Concat(_portfolioExact.GetOffExchangeSnapshot())
                .Select(t => (t.Name,t.AssetCosts, t.Amount))
                .GroupBy(t=>t.Name)
                .Select(t=>(t.Key,t.SelectMany(t2=>t2.AssetCosts),amount: t.Sum(t2=>t2.Amount)));

            var total = 0m;
            var totalLowTax = 0m;

            foreach (var asset in assets)
            {
                var priceEstimationInFiat = _fiatConversion.GetPriceEstimationInFiat(asset.Key, _fiatConversion.LastDate);
                var unrealizedCapitalGain = asset.Item2.Select(t => (priceEstimationInFiat - t.CostPerUnit) * t.RemainingQuantity).Sum();

                if (unrealizedCapitalGain == 0)
                    continue;

                _logger.LogLine($"{asset.Key} qty {asset.amount} unrealized cg {unrealizedCapitalGain:N2} {_currentFiat.Name}");

                foreach (var assetCost in asset.Item2)
                {
                    var unrealizedCapitalGainForAssetCost = (priceEstimationInFiat - assetCost.CostPerUnit) * assetCost.RemainingQuantity;
                    _logger.LogLine($"  {assetCost.Time} {assetCost.Time.ToTimeReadable():dd/MM/yyyy} qty {assetCost.RemainingQuantity} cost base {assetCost.CostPerUnit:N8} unrealized {unrealizedCapitalGainForAssetCost:N2} {_currentFiat.Name}");

                    if ((DateTime.Today - assetCost.Time.ToTimeReadable()).TotalDays >= 365 && unrealizedCapitalGainForAssetCost > 0)
                    {
                        totalLowTax += unrealizedCapitalGainForAssetCost;
                    }
                }

                total += unrealizedCapitalGain;
            }

            _logger.LogLine($"TOTAL {total:N2} {_currentFiat.Name}");
            _logger.LogLine($"TOTAL low tax {totalLowTax:N2} {_currentFiat.Name}");
            _logger.LogLine($"TOTAL high tax {total - totalLowTax:N2} {_currentFiat.Name}");

            return total;
        }
    }
}