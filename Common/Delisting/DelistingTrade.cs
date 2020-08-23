using System;
using Common.Models;
using Common.TradeAbstractions;

namespace Common.Delisting
{
    public class DelistingTrade : ITrade
    {
        public DelistingTrade(DelistingEntry delistingEntry, Dividend dividend, decimal actualAssetValue)
        {
            Time = dividend.DivTime;
            Base = delistingEntry.Base;
            Quote = delistingEntry.Quote;
            IsBuyer = delistingEntry.IsBuy;
            var calculatedQuantity = dividend.Amount / delistingEntry.Price;
            var calculatedVsActualValueDif = Math.Abs(calculatedQuantity - actualAssetValue);
            Quantity = calculatedVsActualValueDif * delistingEntry.Price < 0.00000001m ? actualAssetValue : calculatedQuantity;
            Price = delistingEntry.Price;
            CommissionAsset = "BNB";
            Commission = 0;
        }

        public long Time { get; }
        public string Base { get; } 
        public string Quote { get; } 
        public bool IsBuyer { get; }
        public decimal Quantity { get; }
        public decimal Price { get; }
        public string CommissionAsset { get; } 
        public decimal Commission { get; }
    }
}