using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Common;
using Common.Files;
using Common.Models;
using Common.Rest;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;

namespace MyTradesExtract
{
    public class Program
    {
        public static async Task Main()
        {
            var container = new UnityContainer();
            var unityContainer = new DependencyInjectionContainer(container);

            container.RegisterType<BinanceKeyAndSecret>(new ContainerControlledLifetimeManager());
            container.RegisterFactory<IFileRepositoryConfig>(t => t.Resolve<BinanceKeyAndSecret>());

            var fileRepository = unityContainer.Get<AccountFileRepository>();
            Console.WriteLine($"Starts {DateTime.Now:HH:mm:ss tt zz} - {fileRepository.Account}");
            var tradingRules = await unityContainer.Get<TradingRulesEndPoint>().GetTradingRules();

            var symbols = tradingRules.Symbols.Select(t => t.SymbolName)
                .OrderBy(t=>t)
                .ToArray();

            var allMyTradesEndPoint = unityContainer.Get<AllMyTradesEndPoint>();

            var allTradesSet = symbols.Tag(allMyTradesEndPoint.GetAllTrades);

            using (StreamWriter file = fileRepository.GetTradesStreamWriter())
            {
                int i = 0;
                int j = 0;

                foreach (var allTrades in allTradesSet)
                {
                    var trades = await allTrades.tag.ToList().FirstAsync();
                    foreach (var tradeSet in trades)
                    {
                        foreach (var trade in tradeSet)
                        {
                            if (trade == null)
                                throw new ArgumentException();

                            var tradeString = JsonConvert.SerializeObject(trade);
                            file.WriteLine(tradeString);
                            i++;
                        }
                    }

                    j++;
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss tt zz} - {j}/{symbols.Length} - {allTrades.value}: {trades.Sum(t => t.Count):N0} trade(s) - Total {i:N0}");
                }

                Console.WriteLine($"Finished {DateTime.Now:HH:mm:ss tt zz} - {i:N0} trades");
            }
        }
    }
}