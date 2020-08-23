using System;
using System.Collections.Generic;
using System.Linq;
using Common.Files;
using Common.Models;
using Common.Rest;

namespace Common.Conversions
{
    public class UsdFiatConversion : IFiatConversion
    {
        private readonly Dictionary<(string Asset, DateTime Date), HistoSymbolPrice> _btcPrices;
        private readonly TradingRulesRepository _tradingRulesRepository;
        private readonly Dictionary<(string Asset, DateTime Date), HistoSymbolPrice> _usdtPrices;
        private readonly Dictionary<(string Asset, DateTime Date), HistoSymbolPrice> _busdPrices;

        public UsdFiatConversion(TradingRulesRepository tradingRulesRepository,
            LocalDataRetrieverComposite fileRepository)
        {
            _tradingRulesRepository = tradingRulesRepository;

            var histoSymbolPrices = fileRepository.GetHistoPrices();

            _usdtPrices = histoSymbolPrices
                // we want to get rid of TRYUSDT (turkishLyra to USD) and other entries like that otherwise the conversion would be more complicated with USDT on the other side.
                .Where(t => t.Quote == "USDT")
                .GroupBy(t => (t.Base, t.TimeReadable.Date))
                .Select(t => t.First())
                .ToDictionary(t => (t.Base, t.TimeReadable.Date));

            _busdPrices = histoSymbolPrices
                // we want to get rid of TRYUSDT (turkishLyra to USD) and other entries like that otherwise the conversion would be more complicated with USDT on the other side.
                .Where(t => t.Quote == "BUSD")
                .GroupBy(t => (t.Base, t.TimeReadable.Date))
                .Select(t => t.First())
                .ToDictionary(t => (t.Base, t.TimeReadable.Date));

            // we populate with USDT/USDT
            var dateTimes = _usdtPrices.Keys.Select(t => t.Date).Distinct().ToArray();

            foreach (var dateTime in dateTimes)
                _usdtPrices.Add(("USDT", dateTime), new HistoSymbolPrice {Base = "USDT", Quote = "USDT", Date = dateTime.ToUnixDateTime(), Price = 1m, Symbol = "USDTUSDT"});

            _btcPrices = histoSymbolPrices
                .Where(t => t.Quote == "BTC")
                .GroupBy(t => (t.Base, t.TimeReadable.Date))
                .Select(t => t.First())
                .ToDictionary(t => (t.Base, t.TimeReadable.Date));

            LastDate = _usdtPrices.Max(t => t.Value.Date);
        }

        public long LastDate { get; }

        public string Name => "USD";

        public decimal GetPriceEstimationInFiat(string asset, long time)
        {
            // this method should only be used for non tax generation purpose as it is taking a price estimation
            var date = time.ToTimeReadable().Date;

            if (_usdtPrices.TryGetValue((asset, date), out var result)) return result.Price;

            if (_busdPrices.TryGetValue((asset, date), out var resultBusd)) return resultBusd.Price;

            if (_btcPrices.TryGetValue((asset, date), out var resultBtc) && _usdtPrices.TryGetValue(("BTC", date), out var resultBtcUsdt))
                return resultBtc.Price * resultBtcUsdt.Price;

            // we take the last known price
            var lastKnownUsdtPrice = _usdtPrices.Where(t => t.Key.Asset == asset).OrderBy(t => t.Key.Date).Select(t => t.Value).LastOrDefault();
            if (lastKnownUsdtPrice != null) return lastKnownUsdtPrice.Price;

            var lastKnownUsdtPriceBusd = _busdPrices.Where(t => t.Key.Asset == asset).OrderBy(t => t.Key.Date).Select(t => t.Value).LastOrDefault();
            if (lastKnownUsdtPriceBusd != null) return lastKnownUsdtPriceBusd.Price;

            var lastKnownPrice = _btcPrices.Where(t => t.Key.Asset == asset).OrderBy(t => t.Key.Date).Select(t => t.Value).LastOrDefault();
            if (lastKnownPrice != null) return lastKnownPrice.Price * GetPriceEstimationInFiat("BTC", time);

            if (!_tradingRulesRepository.IsTrading(asset)) return 0;

            throw new Exception($"Cannot convert {asset} for {date:g} {date.ToUnixDateTime()}");
        }
        public decimal? TryGetExactPriceInBtc(string asset, long time)
        {
            var date = time.ToTimeReadable().Date;

            if (_btcPrices.TryGetValue((asset, date), out var result)) return result.Price;

            return null;
        }

        public decimal? TryGetExactPriceInFiat(string asset, long time)
        {
            var date = time.ToTimeReadable().Date;

            var tryGetExactPriceInFiat = _usdtPrices.TryGetValue((asset, date), out var resultSymbol);

            if (tryGetExactPriceInFiat)
                return resultSymbol.Price;

            var tryGetExactPriceInFiatViaBusd = _busdPrices.TryGetValue((asset, date), out var resultSymbolBusd);

            if (tryGetExactPriceInFiatViaBusd)
                return resultSymbolBusd.Price;

            return null;
        }
    }
}