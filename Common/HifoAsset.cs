using System;
using Common.Conversions;
using Common.Models;

using Common.TradeAbstractions;

namespace Common
{
    public sealed class HifoAsset : Asset
    {
        public HifoAsset(AssetName name)  : base(name)
        {
        }

        protected override int GetCostIndex()
        {
            return GetHighestCostIndex();
        }

        private int GetHighestCostIndex()
        {
            if (PreviousCosts.Count == 1)
                return 0;

            int maxIndex = -1;
            decimal maxPrice = -1;

            for (int i = 0; i < PreviousCosts.Count; i++)
            {
                var priceEstimationInFiat = PreviousCosts[i].CostPerUnit;

                if (priceEstimationInFiat > maxPrice)
                {
                    maxIndex = i;
                    maxPrice = priceEstimationInFiat;
                }
            }

            return maxIndex;
        }
    }
}