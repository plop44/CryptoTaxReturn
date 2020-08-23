using System;

namespace Common.Models
{
    public class Candlestick
    {
        public Candlestick(long openTime, decimal open, decimal high, decimal low, decimal close, 
            decimal volume, long closeTime, decimal quoteAssetVolume, int numberOfTrades,
            decimal takerBuyBaseAssetVolume, decimal takerBuyQuoteAssetVolume)
        {
            OpenTime = openTime;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            CloseTime = closeTime;
            QuoteAssetVolume = quoteAssetVolume;
            NumberOfTrades = numberOfTrades;
            TakerBuyBaseAssetVolume = takerBuyBaseAssetVolume;
            TakerBuyQuoteAssetVolume = takerBuyQuoteAssetVolume;
        }

        public long OpenTime { get; }
        public decimal Open { get; }
        public decimal High { get; }
        public decimal Low { get; }
        public decimal Close { get; }
        public decimal Volume { get; }
        public long CloseTime { get; }
        public decimal QuoteAssetVolume { get; }
        public int NumberOfTrades { get; }
        public decimal TakerBuyBaseAssetVolume { get; }
        public decimal TakerBuyQuoteAssetVolume { get; }
        public DateTime OpenTimeTimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(OpenTime).DateTime;
        public DateTime CloseTimeReadable => DateTimeOffset.FromUnixTimeMilliseconds(CloseTime).DateTime;
    }
}
