using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Common.Conversions;
using Common.Delisting;
using Common.Launchpad;
using Common.ModelAbstractions;
using Common.Models;
using Common.TradeAbstractions;

namespace Common
{
    public class Portfolio
    {
        private readonly Dictionary<string, Asset> _assets = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Asset> _withdrawnAssets = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, Asset> _fundOriginAssets = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);
        private readonly IFiatConversion _fiatConversion;
        private readonly Func<AssetName, Asset> _assetFactory;

        public Portfolio(IFiatConversion fiatConversion, Func<AssetName, Asset> assetFactory)
        {
            _fiatConversion = fiatConversion;
            _assetFactory = assetFactory;
        }

        public Asset GetAsset(string name)
        {
            if (!_assets.ContainsKey(name))
                _assets[name] = _assetFactory.Invoke(new AssetName(name));

            return _assets[name];
        }

        public ImmutableArray<CapitalGain> Process(ITrade trade)
        {
            var baseCcy = trade.Base;
            var baseAsset = GetAsset(baseCcy);

            var quoteCcy = trade.Quote;
            var quoteAsset = GetAsset(quoteCcy);

            if (trade.IsBuyer)
            {
                // we are selling trade.GetQuoteQuantity of quoteAsset.
                // to calculate capital gain we need to price this quantity at the time it has been bought
                var proceed = _fiatConversion.GetProceed(trade);

                var assetCost = new AssetCost(trade.Time, trade.Quantity, proceed / trade.Quantity, trade);
                baseAsset.IncrementAmount(assetCost);

                return quoteAsset.DecrementAmount(trade.GetQuoteQuantity(), trade)
                    .Select(t =>
                    {
                        var multiplier = t.RemainingQuantity / trade.GetQuoteQuantity();

                        return new CapitalGain
                        {
                            Asset = quoteCcy,
                            SoldTime = trade.Time,
                            BoughtTime = t.Time,
                            Quantity = t.RemainingQuantity,
                            Cost = t.CostPerUnit * t.RemainingQuantity,
                            Proceed = proceed * multiplier,
                            BoughtTag = GetTag(t.Item)
                        };
                    })
                    .Concat(GetFeeCapitalGains(trade))
                    .ToImmutableArray();
            }
            else
            {
                // we are selling trade.Quantity of baseAsset.
                // to calculate capital gain we need to price this quantity at the time it has been bought
                var proceed = _fiatConversion.GetProceed(trade);

                var assetCost = new AssetCost(trade.Time, trade.GetQuoteQuantity(), proceed / trade.GetQuoteQuantity(), trade);
                quoteAsset.IncrementAmount(assetCost);

                return baseAsset.DecrementAmount(trade.Quantity, trade)
                    .Select(t =>
                    {
                        var multiplier = t.RemainingQuantity / trade.Quantity;

                        return new CapitalGain
                        {
                            Asset = baseCcy,
                            SoldTime = trade.Time,
                            BoughtTime = t.Time,
                            Quantity = t.RemainingQuantity,
                            Cost = t.CostPerUnit * t.RemainingQuantity,
                            Proceed = proceed * multiplier,
                            BoughtTag = GetTag(t.Item)
                        };
                    })
                    .Concat(GetFeeCapitalGains(trade))
                    .ToImmutableArray();
            }
        }

        private IEnumerable<CapitalGain> GetFeeCapitalGains(ITrade trade)
        {
            return GetAsset(trade.CommissionAsset)
                .DecrementAmount(trade.Commission, trade)
                .Select(t => new CapitalGain
                {
                    Asset = trade.CommissionAsset,
                    BoughtTime = t.Time,
                    Quantity = t.RemainingQuantity,
                    SoldTime = trade.Time,
                    Cost = t.CostPerUnit * t.RemainingQuantity,
                    Proceed = 0,
                    BoughtTag = "Trade Fee"
                })
                .ToImmutableArray();
        }

        protected string GetTag(object item)
        {
            switch (item)
            {
                case LaunchpadTrade _: return  "Launchpad";
                case DelistingTrade _: return  "Delisting";
                case Dividend _: return "Dividend";
                case Deposit _: return "Deposit";
                default: return string.Empty;
            }
        }

        public void Process(IDeposit deposit)
        {
            var asset = GetAsset(deposit.Asset);

            var fundsOriginAsset = GetFundOriginAsset(deposit.Asset);
            if (fundsOriginAsset.Amount >= deposit.Amount)
            {
                asset.TransferPosition(fundsOriginAsset, deposit.Amount, deposit.InsertTime, deposit);
                return;
            }

            var withdrawnAsset = GetWithdrawnAsset(deposit.Asset);
            asset.TransferPosition(withdrawnAsset, deposit.Amount, deposit.InsertTime, deposit);
        }

        public ImmutableArray<CapitalGain> Process(DustRow dustRow)
        {
            var sumTransferred = dustRow.Logs.Sum(t => t.TransferedAmount.CleanValueTo8Decimals());
            var sumCharge = dustRow.Logs.Sum(t => t.ServiceChargeAmount.CleanValueTo8Decimals());

            if (sumTransferred != dustRow.TransferedTotal || sumCharge != dustRow.ServiceChargeTotal)
            {
                throw new Exception();
            }

            var result = Enumerable.Empty<CapitalGain>();
            foreach (var dustRowLog in dustRow.Logs)
            {
                var bnbPricePerUnitInFiat = _fiatConversion.GetExactPriceInFiat("BNB", dustRow.OperateTimeLong);
                GetAsset("BNB").IncrementAmount(new AssetCost(dustRowLog.OperateTimeLong, dustRowLog.TransferedAmount, bnbPricePerUnitInFiat, dustRow));

                var proceedInFiat = bnbPricePerUnitInFiat * dustRowLog.TransferedAmount;

                var capitalGains = GetAsset(dustRowLog.FromAsset).DecrementAmount(dustRowLog.Amount, dustRowLog)
                    .Select(t =>
                    {
                        var multiplier = t.RemainingQuantity / dustRowLog.Amount;

                        return new CapitalGain
                        {
                            Asset = dustRowLog.FromAsset,
                            SoldTime = dustRowLog.OperateTimeLong,
                            BoughtTime = t.Time,
                            Quantity = t.RemainingQuantity,
                            Cost = t.CostPerUnit * t.RemainingQuantity,
                            Proceed = proceedInFiat * multiplier,
                            BoughtTag = "Dust Sell"
                        };
                    });

                result = result.Concat(capitalGains);
            }

            return result.ToImmutableArray();
        }

        public ImmutableArray<CapitalGain> Process(IWithdraw withdraw)
        {
            var asset = GetAsset(withdraw.Asset);
            var withdrawnAsset = GetWithdrawnAsset(withdraw.Asset);

            withdrawnAsset.TransferPosition(asset, withdraw.Amount, withdraw.ApplyTime, withdraw);

            // we realize the fees
            return GetFeeCapitalGains(withdraw).ToImmutableArray();
        }


        public void Process(Dividend dividend)
        {
            GetAsset(dividend.Asset)
                .IncrementAmount(new AssetCost(dividend.DivTime, dividend.Amount, 0, dividend));
        }

        public ImmutableArray<Asset> GetAssets()
        {
            return _assets.Values.OrderBy(t => t.Name).ToImmutableArray();
        }

        public void Process(FundsOrigin fundsOrigin)
        {
            var pricePerUnitInFiat = _fiatConversion.GetExactPriceInFiatWithBtcFallbackEnable(fundsOrigin.Asset, fundsOrigin.Time);

            GetFundOriginAsset(fundsOrigin.Asset)
                .IncrementAmount(new AssetCost(fundsOrigin.Time, fundsOrigin.Quantity, pricePerUnitInFiat, fundsOrigin));
        }

        public ImmutableArray<Asset> GetOffExchangeSnapshot()
        {
            return _withdrawnAssets.Values.OrderBy(t => t.Name).ToImmutableArray();
        }

        private Asset GetWithdrawnAsset(string name)
        {
            if (!_withdrawnAssets.ContainsKey(name))
                _withdrawnAssets[name] = _assetFactory.Invoke(new AssetName(name));

            return _withdrawnAssets[name];
        }

        private Asset GetFundOriginAsset(string name)
        {
            if (!_fundOriginAssets.ContainsKey(name))
                _fundOriginAssets[name] = _assetFactory.Invoke(new AssetName(name));

            return _fundOriginAssets[name];
        }

        private IEnumerable<CapitalGain> GetFeeCapitalGains(IWithdraw withdraw)
        {
            return GetAsset(withdraw.Asset)
                .DecrementAmount(withdraw.TransactionFee, withdraw)
                .Select(t => new CapitalGain
                {
                    Asset = withdraw.Asset,
                    BoughtTime = t.Time,
                    Quantity = t.RemainingQuantity,
                    SoldTime = withdraw.ApplyTime,
                    Cost = t.CostPerUnit * t.RemainingQuantity,
                    Proceed = 0,
                    BoughtTag = "Withdraw Fee"
                });
        }
    }
}
