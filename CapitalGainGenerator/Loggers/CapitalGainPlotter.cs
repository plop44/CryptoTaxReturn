using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Controls;
using BackTester.Loggers.WindowLoggers;
using Common;
using Common.Conversions;
using Common.Models;
using Common.Rest;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Wpf;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using DateTimeAxis = OxyPlot.Axes.DateTimeAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace CapitalGainGenerator.Loggers
{
    public sealed class CapitalGainPlotter
    {
        private readonly CurrentFiat _currentFiat;
        private readonly WindowHelper _windowHelper;
        private readonly TaxMethodology _taxMethodology;
        private readonly IFiatConversion _fiatConversion;

        public CapitalGainPlotter(CurrentFiat currentFiat, LoggerFactory loggerFactory, 
            WindowHelper windowHelper, TaxMethodology taxMethodology, IFiatConversion fiatConversion)
        {
            _currentFiat = currentFiat;
            _windowHelper = windowHelper;
            _taxMethodology = taxMethodology;
            _fiatConversion = fiatConversion;
        }

        public void Log(ImmutableArray<CapitalGain> capitalGains)
        {
            var model = new PlotModel
            {
                Title = $"Capital Gain {_currentFiat.Name} {_taxMethodology}"
            };

            decimal previous = 0;
            var serie1 = new LineSeries
            {
                ItemsSource = capitalGains
                    .GroupBy(t => t.SoldTimeReadable.Date)
                    .OrderBy(t => t.Key)
                    .Select(t =>
                    {
                        var x = DateTimeAxis.ToDouble(t.Key);
                        previous += t.Sum(t2 => t2.Gain);
                        return new DataPoint(x, (double) previous);
                    }).ToArray(),
                Title = "Capital gain",
                YAxisKey = "PrimaryAxis"
            };

            var btcSeriePoints = GetBtcSerie(capitalGains[0].SoldTimeReadable, capitalGains[^1].SoldTimeReadable);

            var btcSerie = new LineSeries
            {
                ItemsSource = btcSeriePoints.ToArray(),
                Title = "BTC price",
                YAxisKey = "SecondaryAxis"
            };

            model.Series.Add(serie1);
            model.Series.Add(btcSerie);

            model.Axes.Add(new DateTimeAxis {StringFormat = "MM/yy"});
            model.Axes.Add(new LinearAxis {Position = AxisPosition.Left, Title = $"Gain in {_currentFiat.Name}", StringFormat = "N0", Key = "PrimaryAxis"});
            model.Axes.Add(new LinearAxis {Position = AxisPosition.Right, Title = $"BTC in {_currentFiat.Name}", StringFormat = "N0", Key = "SecondaryAxis"});

            TabItem GetTabItem() => new TabItem
            {
                Header = "Capital Gain",
                Content = new PlotView {Model = model}
            };

            _windowHelper.ShowSeparately(GetTabItem, "Capital Gain");
            _windowHelper.ExportToFiles();
        }

        private IEnumerable<DataPoint> GetBtcSerie(DateTime start, DateTime end)
        {
            while (start<end)
            {
                var priceEstimationInFiat = _fiatConversion.GetPriceEstimationInFiat("BTC", start.ToUnixDateTime());

                yield return new DataPoint(DateTimeAxis.ToDouble(start), (double)priceEstimationInFiat);
                start = start.AddDays(1);
            }
        }
    }
}