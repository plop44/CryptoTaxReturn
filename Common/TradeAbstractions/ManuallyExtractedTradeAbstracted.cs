using Common.Models;
using Common.Rest;

namespace Common.TradeAbstractions
{
    public sealed class ManuallyExtractedTradeAbstracted : ITrade
    {
        private readonly ManuallyExtractedTrade _trade;
        private readonly TradingRulesRepository _tradingRulesRepository;

        public ManuallyExtractedTradeAbstracted(ManuallyExtractedTrade trade, TradingRulesRepository tradingRulesRepository)
        {
            _trade = trade;
            _tradingRulesRepository = tradingRulesRepository;
        }

        public long Time => _trade.Time;
        public string Base => _tradingRulesRepository.GetBaseCcy(_trade.Symbol);
        public string Quote => _tradingRulesRepository.GetQuoteCcy(_trade.Symbol);
        public bool IsBuyer => _trade.IsBuyer;
        public decimal Quantity => _trade.Quantity;
        public decimal Price => _trade.Price;
        public string CommissionAsset => _trade.CommissionAsset;
        public decimal Commission => _trade.Commission;
    }
}