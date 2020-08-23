using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Files;
using Common.Models;
using Common.Rest;
using Newtonsoft.Json;
using Unity;
using Unity.Lifetime;

namespace Extract.PricesAndSymbols
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var container = new UnityContainer();
            var unityContainer = new DependencyInjectionContainer(container);

            container.RegisterType<BinanceKeyAndSecret>(new ContainerControlledLifetimeManager());
            container.RegisterFactory<IFileRepositoryConfig>(t => t.Resolve<BinanceKeyAndSecret>());

            Console.WriteLine($"Starts {DateTime.Now:HH:mm:ss tt zz}");
            Console.WriteLine("This Extract is only needed once. Its data will be used by all accounts.");

            var fileRepository = unityContainer.Get<AccountFileRepository>();

            Console.WriteLine("Saving Trading Rules");

            var tradingRules = await unityContainer.Get<TradingRulesEndPoint>().GetTradingRules();
            fileRepository.SaveTradingRules(tradingRules);

            Console.WriteLine("Saving Histo prices");
            var ccyPairs = tradingRules.Symbols
                .Where(t => t.QuoteAsset == "USDT" || t.BaseAsset == "USDT" || t.QuoteAsset == "BTC" || t.BaseAsset == "BTC" || t.QuoteAsset == "BUSD" || t.BaseAsset == "BUSD")
                .ToArray();

            var candleStickEndpoint = unityContainer.Get<CandlesticksEndPoint>();

            await using StreamWriter file = fileRepository.GetHistoPricesStreamWriter();
            var i = 0;
            foreach (var ccyPair in ccyPairs)
            {
                var candles = await candleStickEndpoint.GetCandlesticks(ccyPair.SymbolName, TimeInterval.Days_1, limit: 1000);

                foreach (var candlestick in candles)
                {
                    var openTime = candlestick.OpenTimeTimeReadable.Date;
                    var closeDate = candlestick.CloseTimeReadable.Date;

                    if (openTime != closeDate)
                    {
                        Console.WriteLine($"Candle not valid for {ccyPair.SymbolName}");
                        continue;
                    }

                    var usdtPrice = new HistoSymbolPrice
                    {
                        Base = ccyPair.BaseAsset,
                        Quote = ccyPair.QuoteAsset,
                        Date = closeDate.ToUnixDateTime(),
                        Symbol = ccyPair.SymbolName,
                        Price = candlestick.Close
                    };
                    var serializeObject = JsonConvert.SerializeObject(usdtPrice);
                    file.WriteLine(serializeObject);
                }

                i++;

                if (i % 20 == 0)
                    Console.WriteLine($"Saved {i}/{ccyPairs.Length}");
            }

            Console.WriteLine($"Saved {i}/{ccyPairs.Length}");
        }
    }
}
