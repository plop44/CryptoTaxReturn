using System;
using Common.Models;

namespace Common
{
    public sealed class AssetCost
    {
        public AssetCost(long time, decimal remainingQuantity, decimal costPerUnit, object item)
        {
            Time = time;
            RemainingQuantity = remainingQuantity.CleanValueTo8Decimals();
            CostPerUnit = costPerUnit;
            Item = item;

            if (RemainingQuantity <= 0)
                throw new Exception("RemainingQuantity should be positive");
        }

        public long Time { get; }
        public decimal RemainingQuantity { get; private set; }
        public decimal CostPerUnit { get; }
        public object Item { get;  }

        public AssetCost Split(decimal extractValue)
        {
            if(extractValue<0)
                throw new ArgumentException($"{nameof(extractValue)} should be positive");

            if (extractValue > RemainingQuantity)
                throw new ArgumentException($"{nameof(extractValue)} should be smaller than {nameof(RemainingQuantity)}");

            RemainingQuantity -= extractValue;

            return new AssetCost(Time, extractValue, CostPerUnit, Item);
        }
    }
}