using System;
using System.Threading.Tasks;
using Common;
using Common.Files;
using Common.Rest;
using Unity;
using Unity.Lifetime;

namespace Extract.AllOthers
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = new UnityContainer();
            var unityContainer = new DependencyInjectionContainer(container);

            container.RegisterType<BinanceKeyAndSecret>(new ContainerControlledLifetimeManager());
            container.RegisterFactory<IFileRepositoryConfig>(t=>t.Resolve<BinanceKeyAndSecret>());

            var depositHistoryEndPoint = unityContainer.Get<DepositHistoryEndPoint>();
            var fileRepository = unityContainer.Get<AccountFileRepository>();
            Console.WriteLine($"Starts {DateTime.Now:HH:mm:ss tt zz} - {fileRepository.Account}");

            Console.WriteLine("Saving AccountInfo");

            var accountInfo = await unityContainer.Get<AccountInfoEndPoint>().GetAccountInfo();
            fileRepository.SaveAccountInfo(accountInfo);

            Console.WriteLine("Saving Withdraw");

            var withdrawHistory = await depositHistoryEndPoint.GetWithdrawHistory(new DateTime(2017,01,01));
            fileRepository.SaveWithdraws(withdrawHistory);

            Console.WriteLine("Saving Deposit");

            var depositHistory = await depositHistoryEndPoint.GetDepositHistory(new DateTime(2017,01,01));
            fileRepository.SaveDeposits(depositHistory);

            Console.WriteLine("Saving Dividend");

            var dividendHistory = await depositHistoryEndPoint.GetDividendHistory(new DateTime(2017, 01, 01));
            fileRepository.SaveDividends(dividendHistory);

            Console.WriteLine("Saving Dust");

            var dustHistory = await depositHistoryEndPoint.GetDustHistory();
            fileRepository.SaveDusts(dustHistory);
        }
    }
}
