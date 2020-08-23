using System;
using System.Collections.Generic;
using System.Linq;
using Common.Files;
using Common.Models;

namespace Common.Rest
{
    public class TradingRulesRepository
    {
        private readonly Dictionary<string, (string baseCcy, string quoteCcy)> _symbolToQuoteAndBaseCurrencies = new Dictionary<string, (string baseCcy, string quoteCcy)>();
        private readonly List<Symbol> _symbols = new List<Symbol>();
        private readonly HashSet<string> _tradingSymbols;

        public TradingRulesRepository(LocalDataRetrieverComposite fileRepository)
        {
            var tradingRules = fileRepository.GetTradingRules();

            if (tradingRules == null)
            {
                _tradingSymbols = new HashSet<string>();
                CurrencyPairs = new List<(string ccyPair, string baseCcy, string quoteCcy)>();
                return;
            }

            var altCurrencies = new HashSet<string>();
            var pivotCurrencies = new HashSet<string>();
            var currencyPairs = new List<(string ccyPair, string baseCcy, string quoteCcy)>();

            _symbols.AddRange(tradingRules.Symbols);

            foreach (var symbol in tradingRules.Symbols)
            {
                currencyPairs.Add((symbol.SymbolName, symbol.BaseAsset, symbol.QuoteAsset));
                altCurrencies.Add(symbol.BaseAsset);
                pivotCurrencies.Add(symbol.QuoteAsset);
                _symbolToQuoteAndBaseCurrencies[symbol.SymbolName] = (symbol.BaseAsset, symbol.QuoteAsset);
            }

            _tradingSymbols = tradingRules.Symbols.Where(t => t.Status == "TRADING").SelectMany(t => new[] { t.BaseAsset, t.QuoteAsset }).Distinct().ToHashSet();

            // because this trading pair has been existing but disappeared
            _symbolToQuoteAndBaseCurrencies["USDCBTC"] = ("USDC", "BTC");
            _symbolToQuoteAndBaseCurrencies["CTRBTC"] = ("CTR", "BTC");
            _symbolToQuoteAndBaseCurrencies["CTRETH"] = ("CTR", "ETH");

            CurrencyPairs = currencyPairs.OrderBy(t => t).ToArray();
        }

        public IReadOnlyCollection<(string ccyPair, string baseCcy, string quoteCcy)> CurrencyPairs { get; }

        public bool IsTrading(string crypto)
        {
            return _tradingSymbols.Contains(crypto);
        }

        public string GetQuoteCcy(string symbol)
        {
            return _symbolToQuoteAndBaseCurrencies.TryGetValue(symbol, out var result) ? result.quoteCcy : throw new Exception();
        }

        public string GetBaseCcy(string symbol)
        {
            return _symbolToQuoteAndBaseCurrencies.TryGetValue(symbol, out var result) ? result.baseCcy : throw new Exception();
        }

        public bool IsSymbolValid(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return false;

            return _symbolToQuoteAndBaseCurrencies.ContainsKey(symbol);
        }

        public string GetStatus(string asset)
        {
            return _symbols
                .Where(t => t.BaseAsset == asset || t.QuoteAsset == asset)
                .Select(t => t.Status)
                .Distinct()
                .JoinString(",");
        }
    }
}