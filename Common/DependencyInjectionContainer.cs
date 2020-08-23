using System;
using System.IO;
using Common.Conversions;
using Common.Files;
using Common.Models;
using Common.Rest;
using Common.TradeAbstractions;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;
using Unity.Resolution;

namespace Common
{
    public interface ITaxMethodologyConfig
    {
        TaxMethodology TaxMethodology { get; }
    }
    public class DependencyInjectionContainer
    {
        private readonly IUnityContainer _unityContainer;
        public DependencyInjectionContainer(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer.AddExtension(new Diagnostic());

            _unityContainer.RegisterType<CurrentFiat>(new ContainerControlledLifetimeManager());
            _unityContainer.RegisterFactory<TaxMethodology>(t=>t.Resolve<ITaxMethodologyConfig>().TaxMethodology);

            _unityContainer.RegisterFactory<PathsConfig>(t => JsonConvert.DeserializeObject<PathsConfig>(File.ReadAllText(@".\PathsConfig.json")));

            _unityContainer.RegisterFactory<IFiatConversion>(GetFiatConversionPrivate, new ContainerControlledLifetimeManager());
            _unityContainer.RegisterType<TradingRulesRepository>(new ContainerControlledLifetimeManager());
            _unityContainer.RegisterType<BinanceRestClient>(new ContainerControlledLifetimeManager());
            _unityContainer.RegisterInstance<Func<Trade, TradeAbstracted>>(t => _unityContainer.Resolve<TradeAbstracted>(new DependencyOverride<Trade>(t)));
            _unityContainer.RegisterInstance<Func<ManuallyExtractedTrade, ManuallyExtractedTradeAbstracted>>(t => _unityContainer.Resolve<ManuallyExtractedTradeAbstracted>(new DependencyOverride<ManuallyExtractedTrade>(t)));
            _unityContainer.RegisterType<Portfolio>(new ContainerControlledLifetimeManager());
            _unityContainer.RegisterType<PortfolioAcrossTime>(new ContainerControlledLifetimeManager());
            _unityContainer.RegisterFactory<Func<AssetName, Asset>>(GetAssetFactory, new ContainerControlledLifetimeManager());
        }

        private Func<AssetName, Asset> GetAssetFactory(IUnityContainer unityContainer)
        {
            var taxMethodology = _unityContainer.Resolve<TaxMethodology>();
            switch (taxMethodology)
            {
                case TaxMethodology.Fifo: return t => _unityContainer.Resolve<FifoAsset>(new DependencyOverride<AssetName>(t));
                case TaxMethodology.Lifo: return t => _unityContainer.Resolve<LifoAsset>(new DependencyOverride<AssetName>(t));
                case TaxMethodology.Hifo: return t => _unityContainer.Resolve<HifoAsset>(new DependencyOverride<AssetName>(t));
            }

            throw new ArgumentException($"{taxMethodology} not implemented");
        }

        private static IFiatConversion GetFiatConversionPrivate(IUnityContainer unityContainer)
        {
            IFiatConversion toReturn = unityContainer.Resolve<UsdFiatConversion>();

            if (unityContainer.Resolve<CurrentFiat>().Name == "AUD")
                toReturn = unityContainer.Resolve<AudFiatConversion>(new DependencyOverride<UsdFiatConversion>(toReturn));

            var config = unityContainer.Resolve<IPortfolioAcrossTimeConfig>();
            if (config.IsNewTaxCitizenStartDateApplies)
            {
                toReturn = new FiatConversionWithMinDate(toReturn, config);
            }

            return toReturn;
        }

        public T Get<T>()
        {
            return _unityContainer.Resolve<T>();
        }
    }
}