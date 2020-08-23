using Common.Models;
using Common.Rest;

namespace Common.TradeAbstractions
{
    public sealed class TradeAbstracted : ITrade
    {
        private readonly Trade _trade;
        private readonly TradingRulesRepository _tradingRulesRepository;

        public TradeAbstracted(Trade trade, TradingRulesRepository tradingRulesRepository)
        {
            _trade = trade;
            _tradingRulesRepository = tradingRulesRepository;
            Base = _tradingRulesRepository.GetBaseCcy(_trade.Symbol);
            Quote = _tradingRulesRepository.GetQuoteCcy(_trade.Symbol);
        }

        public string Base { get; }
        public string Quote { get; }
        public bool IsBuyer => _trade.IsBuyer;
        public decimal Quantity => _trade.Quantity;
        public decimal Price => _trade.Price;
        public string CommissionAsset => _trade.CommissionAsset;
        public decimal Commission => _trade.Commission;
        public long Time => _trade.Time;
    }
}