using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.RestCalls;
using Common.RestCalls.Rest;
using Newtonsoft.Json;

namespace AddingRates
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Starts {DateTime.Now:HH:mm:ss tt zz}");
            var unityContainer = new DependencyInjectionContainer();

            var tradingRulesRepository = unityContainer.GetTradingRulesRepository();

            var tag = unityContainer.GetTag();

            var file = "faka";
            var trades = File.ReadLines($"C:\\temp\\Taxes\\{tag}\\Trades.txt")
                .Select(JsonConvert.DeserializeObject<TradeWithRates>);

            var audUsdFxRate = new AudUsdFxRate();
            var histoPrice = unityContainer.GetHistoPrice();

            Console.WriteLine("Starting caching candle for histo");
            await histoPrice.IsInitialized;
            Console.WriteLine("Caching candle for histo done");

            await using StreamWriter fileStream = new StreamWriter($@"C:\temp\Taxes\{tag}\temp\TradesWithRates{Guid.NewGuid().ToString().Substring(0, 4)}.txt");

            int i = 0;
            foreach (var trade in trades)
            {
                trade.AudToUsdRate = audUsdFxRate.GetRate(trade.TimeReadable);
                var rates = await histoPrice.GetRates(trade);
                if (rates.commissionRate == 0 || rates.quoteRate == 0)
                    throw new Exception("???");
                trade.CommissionAssetToUsdtRate = rates.commissionRate;
                trade.QuoteToUsdtRate = rates.quoteRate;

                var tradeString = JsonConvert.SerializeObject(trade);
                fileStream.WriteLine(tradeString);
                i++;

                if (i % 100000 == 0)
                    Console.WriteLine($"{DateTime.Now:HH:mm:ss tt zz} - {i:N0} trade processed");
            }
        }
    }
}