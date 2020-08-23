using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Common;
using Common.Files;
using Common.Rest;
using Unity;
using Unity.Lifetime;

namespace Extract.KuCoin
{
    class Program
    {
        static async Task Main()
        {
            var container = new UnityContainer();
            var unityContainer = new DependencyInjectionContainer(container);

            container.RegisterType<KuCoinKeyAndSecret>(new ContainerControlledLifetimeManager());
            container.RegisterFactory<IFileRepositoryConfig>(t => t.Resolve<KuCoinKeyAndSecret>());

            var accountFileRepository = unityContainer.Get<AccountFileRepository>();
            Console.WriteLine($"Starts KuCoin Extract for account {accountFileRepository.Account}");
            var kuCoinEndPoints = unityContainer.Get<KuCoinEndPoints>();

            var trades = await kuCoinEndPoints.GetAllTrades(new DateTime(2020, 01, 01));
            accountFileRepository.Save(trades);

            var deposits = await kuCoinEndPoints.GetDeposit();
            accountFileRepository.Save(deposits);

            var withdrawals = await kuCoinEndPoints.GetWithdrawals();
            accountFileRepository.Save(withdrawals);

            var balances = await kuCoinEndPoints.GetBalances();
            accountFileRepository.Save(balances.ToImmutableArray());
        }
    }
}
