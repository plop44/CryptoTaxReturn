using System;
using System.IO;
using CapitalGainGenerator.Loggers;
using Common;
using Common.Files;
using Common.Rest;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;
using Unity.Resolution;

namespace CapitalGainGenerator
{
    class Program
    {
        static void Main()
        {
            //create readme
            Console.WriteLine($"Starts {DateTime.Now:HH:mm:ss tt zz} - Capital gain calculation for tax purpose");
            var unityContainer = new UnityContainer();
            var dependencyInjectionContainer = new DependencyInjectionContainer(unityContainer);

            unityContainer.RegisterInstance(JsonConvert.DeserializeObject<Config>(File.ReadAllText(@".\Config.json")));
            unityContainer.RegisterFactory<ICurrentFiatConfig>(t=>t.Resolve<Config>());
            unityContainer.RegisterFactory<ILocalDataRetrieverCompositeConfig>(t=>t.Resolve<Config>());
            unityContainer.RegisterFactory<IPortfolioAcrossTimeConfig>(t=>t.Resolve<Config>());
            unityContainer.RegisterFactory<ITaxMethodologyConfig>(t=>t.Resolve<Config>());

            unityContainer.RegisterType<LocalDataRetrieverComposite>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<CapitalGainReportSaver>(new ContainerControlledLifetimeManager());
            unityContainer.RegisterType<LoggerFactory>(new ContainerControlledLifetimeManager());

            unityContainer.RegisterInstance<Func<LocalDataRetrieverComposite.ManuallySetFileRepositoryConfig, AccountFileRepository>>(t =>
                unityContainer.Resolve<AccountFileRepository>(new DependencyOverride<IFileRepositoryConfig>(t)));

            dependencyInjectionContainer.Get<EndOfFinancialYearPortfolioLogger>();
            var currentFiat = dependencyInjectionContainer.Get<CurrentFiat>();
            var localDataRetriever = dependencyInjectionContainer.Get<LocalDataRetrieverComposite>();

            Console.WriteLine($"Accounts {localDataRetriever.Account}");
            Console.WriteLine($"Currency {currentFiat.Name}, {dependencyInjectionContainer.Get<TaxMethodology>()}");

            var portfolioAcrossTime = dependencyInjectionContainer.Get<PortfolioAcrossTime>();
            portfolioAcrossTime.Run();

            var capitalGains = portfolioAcrossTime.GetCapitalGains();
            dependencyInjectionContainer.Get<CapitalGainPlotter>().Log(capitalGains);
            dependencyInjectionContainer.Get<BalanceDifferencesPortfolioLogger>().LogDifferences();
            dependencyInjectionContainer.Get<PortfolioBreachesLogger>().Log();
            var financialYearLogger = dependencyInjectionContainer.Get<TaxReportLogger>();
            var totalCapitalGain = financialYearLogger.Log(capitalGains);
            dependencyInjectionContainer.Get<CapitalGainPerCategoryLogger>().Log(capitalGains);
            dependencyInjectionContainer.Get<CapitalGainPerAssetLogger>().Log(capitalGains);
            dependencyInjectionContainer.Get<InputOutputLogger>().Log();
            dependencyInjectionContainer.Get<OffExchangeAssetPortfolioLogger>().Log();
            var totalUnrealized = dependencyInjectionContainer.Get<UnrealizedCapitalGainLogger>().Log();

            financialYearLogger.LogLine($"\nUnrealized {totalUnrealized:N2} {currentFiat.Name}");
            financialYearLogger.LogLine($"\nCapital Gain + unrealized {totalCapitalGain + totalUnrealized:N2} {currentFiat.Name}");
        }
    }
}