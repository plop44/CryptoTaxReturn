using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Model;
using Common.RestCalls;
using Common.RestCalls.Rest;

namespace AddingRates
{
    public class HistoPrice
    {
        private readonly Dictionary<(DateTime, string), decimal> _cache = new Dictionary<(DateTime, string), decimal>();
        private readonly CandlesticksEndPoint _candlesticksEndPoint;
        private readonly TradingRulesRepository _tradingRulesRepository;

        public HistoPrice(TradingRulesRepository tradingRulesRepository,
            CandlesticksEndPoint candlesticksEndPoint)
        {
            _tradingRulesRepository = tradingRulesRepository;
            _candlesticksEndPoint = candlesticksEndPoint;
            IsInitialized = CacheValues();
        }

        public Task IsInitialized { get; }

        private async Task CacheValues()
        {
            var ccyPairs = _tradingRulesRepository.CurrencyPairs
                .Where(t => t.quoteCcy == "USDT" || t.quoteCcy == "BTC")
                //.Where(t=>t.baseCcy=="EDO" || t.baseCcy=="BTC" || t.baseCcy=="BNB"|| t.baseCcy=="ETH"|| t.baseCcy=="TUSD"|| t.baseCcy=="PAX"|| t.baseCcy=="USDC"|| t.baseCcy=="BUSD")
                .ToArray();

            var i = 0;
            foreach (var ccyPair in ccyPairs)
            {
                var candles = await _candlesticksEndPoint.GetCandlesticks(ccyPair.ccyPair, TimeInterval.Days_1, limit: 1000);

                if (candles == null || candles.Length == 0)
                    throw new Exception($"Currency pair {ccyPair} does not exists");

                foreach (var candlestick in candles) _cache[(candlestick.CloseTimeReadable.Date, ccyPair.ccyPair)] = candlestick.Close;

                i++;

                if (i % 20 == 0)
                    Console.WriteLine($"Cached {i}/{ccyPairs.Length}");
            }

            Console.WriteLine($"Cached {i}/{ccyPairs.Length}");
        }

        public async Task<(decimal commissionRate, decimal quoteRate)> GetRates(TradeWithRates trade)
        {
            var commissionAssetRate = await GetRate(trade.TimeReadable, trade.CommissionAsset);

            var quoteCcy = _tradingRulesRepository.GetQuoteCcy(trade.Symbol);

            if (string.IsNullOrEmpty(quoteCcy))
                throw new Exception("???");

            if (quoteCcy == trade.CommissionAsset) return (commissionAssetRate, commissionAssetRate);

            var quoteCcyAssetRate = await GetRate(trade.TimeReadable, quoteCcy);

            return (commissionAssetRate, quoteCcyAssetRate);
        }

        private async Task<decimal> GetRate(DateTime tradeTime, string baseCcy, string quoteCurrency = "USDT")
        {
            if (baseCcy == quoteCurrency)
                return 1;

            if (_cache.TryGetValue((tradeTime.Date, baseCcy + quoteCurrency), out var result))
                return result;

            if (quoteCurrency == "BTC")
                throw new Exception($"cannot translate {baseCcy} to USDT");

            var rate1 = await GetRate(tradeTime, baseCcy, "BTC");
            var rate2 = await GetRate(tradeTime, "BTC");

            return rate1 * rate2;
        }
    }
}