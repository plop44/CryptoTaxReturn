using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Common.Models;
using Common.TradeAbstractions;

namespace Common
{
    public abstract class Asset
    {
        private readonly List<string> _breachLogs = new List<string>();
        protected readonly List<AssetCost> PreviousCosts = new List<AssetCost>();


        protected Asset(AssetName name)
        {
            Name = name.Name;
        }

        public ImmutableArray<AssetCost> AssetCosts => PreviousCosts.ToImmutableArray();
        public ImmutableArray<string> GetBreachLogs => _breachLogs.ToImmutableArray();

        public string Name { get; }

        public bool HasBreached => BreachCount != 0 || BreachedAmount != 0 || SellSideBreaches != 0 || _breachLogs.Count != 0;
        public int BreachCount { get; private set; }
        public decimal BreachedAmount { get; private set; }
        public int ResolvedBreachCount { get; private set; }
        public int SellSideBreaches { get; private set; }
        public decimal Amount { get; private set; }

        public void IncrementAmount(AssetCost assetCost)
        {
            Amount += assetCost.RemainingQuantity;

            PreviousCosts.Add(assetCost);

            if (Amount - assetCost.RemainingQuantity < 0 && Amount >= 0)
            {
                ResolvedBreachCount++;

                if (assetCost.Item is ITrade trade)
                {
                    var buyText = trade.IsBuyer ? "BUY" : "SELL";
                    _breachLogs.Add($"debreach {trade.Time} {trade.GetTimeReadable():G} {trade.GetSymbol()} {buyText} {Name} {trade.Commission}");
                }
            }
        }

        public ImmutableArray<AssetCost> DecrementAmount(decimal value, object item)
        {
            var cleanedValue = value.CleanValueTo8Decimals();

            if (cleanedValue == 0)
                return ImmutableArray<AssetCost>.Empty;

            if (cleanedValue < 0)
                throw new Exception("Increment amount should be called");

            Amount -= cleanedValue;

            if (Amount < 0)
            {
                BreachCount++;
                BreachedAmount += -Amount;

                if (item is ITrade trade)
                {
                    var buyText = trade.IsBuyer ? "BUY" : "SELL";
                    _breachLogs.Add($"breach {trade.Time} {trade.GetTimeReadable():G} {trade.GetSymbol()} {buyText} {Name} {trade.Commission}");
                }
            }

            var sellSideFor = ExtractCost(cleanedValue).ToImmutableArray();

            if (sellSideFor.Sum(t => t.RemainingQuantity) != cleanedValue)
                SellSideBreaches++;

            return sellSideFor;
        }

        protected abstract int GetCostIndex();

        private IEnumerable<AssetCost> ExtractCost(decimal quantity)
        {
            if (quantity < 0)
                throw new Exception();

            if (quantity == 0)
                yield break;

            var remainingToReturn = quantity;

            while (remainingToReturn > 0 && PreviousCosts.Count > 0)
            {
                var costIndex = GetCostIndex();

                var assetCost = PreviousCosts[costIndex];
                PreviousCosts.RemoveAt(costIndex);

                if (assetCost.RemainingQuantity > remainingToReturn)
                {
                    var split = assetCost.Split(remainingToReturn);
                    yield return split;
                    PreviousCosts.Insert(costIndex, assetCost);
                    yield break;
                }

                if (assetCost.RemainingQuantity == remainingToReturn)
                {
                    yield return assetCost;
                    yield break;
                }

                yield return assetCost;
                remainingToReturn -= assetCost.RemainingQuantity;
            }
        }

        public void TransferPosition(Asset transferFrom, decimal quantity, long time, object item)
        {
            if (quantity < 0)
                throw new ArgumentException("Transfer should be the other way around");

            var sellSideFor = transferFrom.DecrementAmount(quantity, item);

            var transferReceived = sellSideFor.Sum(t => t.RemainingQuantity);

            if (transferReceived != quantity)
            {
                Console.WriteLine($"No funds origin for transfer {Name} = {quantity} @ {time} {time.ToTimeReadable():g}");
                Console.WriteLine("Please add it, currently cost for this asset will be 0.");
                sellSideFor = sellSideFor.Add(new AssetCost(time, quantity - transferReceived, 0, new NoOriginTransfer(time)));
            }

            Amount += quantity;

            PreviousCosts.AddRange(sellSideFor);
        }

        public override string ToString()
        {
            return $"{Name} => {Amount:N8}";
        }
    }
}