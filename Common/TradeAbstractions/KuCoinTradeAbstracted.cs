using System;
using Common.Models;

namespace Common.TradeAbstractions
{
    public sealed class KuCoinTradeAbstracted : ITrade
    {
        private readonly KuCoinTrade _trade;

        public KuCoinTradeAbstracted(KuCoinTrade trade)
        {
            _trade = trade;
        }

        public string Base => _trade.Symbol.Split('-')[0];
        public string Quote => _trade.Symbol.Split('-')[1];
        public bool IsBuyer => !_trade.Side.Equals("sell",StringComparison.CurrentCultureIgnoreCase);
        public decimal Quantity => _trade.Size;
        public decimal Price => _trade.Price;
        public string CommissionAsset => _trade.FeeCurrency;
        public decimal Commission => _trade.Fee;
        public long Time => _trade.CreatedAt;
    }
}