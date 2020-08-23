using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Common.ModelAbstractions;
using Common.Models;
using Common.TradeAbstractions;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace Common.Files
{
    public class AccountFileRepository
    {
        public const string CommonFolderName = "Common";
        private const string ManualExtractTrades = "ManualExtractTrades*";
        private const string BinanceTrades = "Trades*";
        private readonly string _accountInfoFileName;
        private readonly string _depositFileName;
        private readonly string _dividendsFileName;
        private readonly string _dustsFileName;
        private readonly string _histoPricesFileName;
        private readonly string _histoPricesManualFileName;
        private readonly string _kcBalancesFileName;
        private readonly string _kcDepositFileName;
        private readonly string _kcTradesFileName;
        private readonly string _kcWithdrawFileName;
        private readonly string _tag;

        private readonly Func<Trade, TradeAbstracted> _tradeAbstractedFactory;
        private readonly string _tradesFileName;
        private readonly string _tradingRulesFileName;

        private readonly Func<ManuallyExtractedTrade, ManuallyExtractedTradeAbstracted> _usdcBtcTradeAbstractedFactory;
        private readonly string _withdrawFileName;
        private readonly string _accountDirectory;

        public AccountFileRepository(IFileRepositoryConfig fileRepositoryConfig, 
            Func<Trade, TradeAbstracted> tradeAbstractedFactory,
            Func<ManuallyExtractedTrade, ManuallyExtractedTradeAbstracted> usdcBtcTradeAbstractedFactory,
            PathsConfig pathsConfig)
        {
            _tradeAbstractedFactory = tradeAbstractedFactory;
            _usdcBtcTradeAbstractedFactory = usdcBtcTradeAbstractedFactory;
            _tag = fileRepositoryConfig.Account;

            var commonDirectory = Path.Combine(pathsConfig.ExtractFolder, CommonFolderName);
            if (!Directory.Exists(commonDirectory))
                Directory.CreateDirectory(commonDirectory);

            _accountDirectory = Path.Combine(pathsConfig.ExtractFolder, _tag);
            if (!Directory.Exists(_accountDirectory))
                Directory.CreateDirectory(_accountDirectory);

            _accountInfoFileName = Path.Combine(_accountDirectory, "AccountInfo.json");
            _depositFileName = Path.Combine(_accountDirectory, "Deposit.json");
            _kcDepositFileName = Path.Combine(_accountDirectory, "KcDeposit.json");
            _withdrawFileName = Path.Combine(_accountDirectory, "Withdraw.json");
            _kcWithdrawFileName = Path.Combine(_accountDirectory, "KcWithdraw.json");
            _dividendsFileName = Path.Combine(_accountDirectory, "Dividends.json");
            _dustsFileName = Path.Combine(_accountDirectory, "Dusts.json");
            _tradingRulesFileName = Path.Combine(commonDirectory, "TradingRules.json");
            _histoPricesFileName = Path.Combine(commonDirectory, "HistoPrices.json");
            _histoPricesManualFileName = Path.Combine(_accountDirectory, "HistoPricesManual.json");
            _kcTradesFileName = Path.Combine(_accountDirectory, "KcTrades.json");
            _kcBalancesFileName = Path.Combine(_accountDirectory, "KcBalances.json");
            _tradesFileName = Path.Combine(_accountDirectory, "Trades.json");
                        Account = _tag;
        }

        public string Account { get; }

        public ImmutableArray<IWithdraw> GetWithdraws()
        {
            IEnumerable<IWithdraw> withdraws = Enumerable.Empty<IWithdraw>();

            if (File.Exists(_withdrawFileName))
                withdraws = withdraws.Concat(File.ReadAllLines(_withdrawFileName)
                    .Select(JsonConvert.DeserializeObject<Withdraw>)
                    .Select(t => (IWithdraw) new WithdrawAbstracted(t)));

            if (File.Exists(_kcWithdrawFileName))
                withdraws = withdraws.Concat(File.ReadAllLines(_kcWithdrawFileName)
                    .Select(JsonConvert.DeserializeObject<KuCoinWithdrawal>)
                    .Select(t => (IWithdraw) new KuCoinWithdrawAbstracted(t)));

            return withdraws
                .ToImmutableArray();
        }

        public ImmutableArray<IDeposit> GetDeposits()
        {
            IEnumerable<IDeposit> deposits = Enumerable.Empty<IDeposit>();

            if (File.Exists(_depositFileName))
                deposits = deposits.Concat(File.ReadAllLines(_depositFileName)
                    .Select(JsonConvert.DeserializeObject<Deposit>)
                    .Select(t => (IDeposit) new DepositAbstracted(t)));

            if (File.Exists(_kcDepositFileName))
                deposits = deposits.Concat(File.ReadAllLines(_kcDepositFileName)
                    .Select(JsonConvert.DeserializeObject<KuCoinDeposit>)
                    .Select(t => (IDeposit) new KuCoinDepositAbstracted(t)));

            return deposits.ToImmutableArray();
        }

        public ImmutableArray<Dividend> GetDividends()
        {
            if (File.Exists(_dividendsFileName))
                return File.ReadAllLines(_dividendsFileName)
                    .Select(JsonConvert.DeserializeObject<Dividend>)
                    .ToImmutableArray();

            return ImmutableArray<Dividend>.Empty;
        }

        public ImmutableArray<DustRow> GetDusts()
        {
            if (File.Exists(_dustsFileName))
                return File.ReadAllLines(_dustsFileName)
                    .Select(JsonConvert.DeserializeObject<DustRow>)
                    .ToImmutableArray();

            return ImmutableArray<DustRow>.Empty;
        }

        public ImmutableArray<ITrade> GetTrades()
        {
            IEnumerable<ITrade> trades = GetBinanceTrades();

            if (File.Exists(_kcTradesFileName))
                trades = trades.Concat(File.ReadLines(_kcTradesFileName)
                    .Select(JsonConvert.DeserializeObject<KuCoinTrade>)
                    .Select(t => (ITrade) new KuCoinTradeAbstracted(t)));

            return trades
                .Concat(GetManuallyExtractedTrades().Select(t => (ITrade) _usdcBtcTradeAbstractedFactory.Invoke(t)))
                .ToImmutableArray();
        }

        public ImmutableArray<Balance> GetAccountInfoBalances()
        {
            IEnumerable<Balance> balances = Enumerable.Empty<Balance>();

            if (File.Exists(_accountInfoFileName))
                balances = balances.Concat(JsonConvert.DeserializeObject<AccountInfo>(File.ReadAllText(_accountInfoFileName)).Balances);

            if (File.Exists(_kcBalancesFileName))
                balances = balances.Concat(File.ReadAllLines(_kcBalancesFileName)
                    .Select(JsonConvert.DeserializeObject<KcBalance>)
                    .GroupBy(t => t.Currency)
                    .Select(t => new Balance
                    {
                        Asset = t.Key,
                        Free = t.Sum(t2 => t2.Available),
                        Locked = t.Sum(t2 => t2.Holds)
                    }));

            return balances.GroupBy(t => t.Asset).Select(t => new Balance {Asset = t.Key, Free = t.Sum(t2 => t2.Free), Locked = t.Sum(t2 => t2.Locked)}).ToImmutableArray();
        }

        public TradingRules GetTradingRules()
        {
            var readAllText = File.ReadAllText(_tradingRulesFileName);
            return JsonConvert.DeserializeObject<TradingRules>(readAllText);
        }

        public ImmutableArray<HistoSymbolPrice> GetHistoPrices()
        {
            var toReturn = File.ReadLines(_histoPricesFileName)
                .Select(JsonConvert.DeserializeObject<HistoSymbolPrice>);

            if (File.Exists(_histoPricesManualFileName))
                toReturn = toReturn.Concat(File.ReadAllLines(_histoPricesManualFileName)
                    .Where(t => !t.StartsWith("IGNORE"))
                    .Select(JsonConvert.DeserializeObject<HistoSymbolPrice>));

            return toReturn.ToImmutableArray();
        }

        public ImmutableArray<FundsOrigin> GetFundsOrigin()
        {
            return File.ReadLines("./YourHomework/FundsOrigin.csv")
                .Skip(1)
                .Where(t=>!t.StartsWith("//"))
                .Select(t => new FundsOrigin(t))
                .ToImmutableArray();
        }

        public void SaveAccountInfo(AccountInfo accountInfo)
        {
            var accountInfoJson = JsonConvert.SerializeObject(accountInfo);
            File.WriteAllText(GetFileName(_accountInfoFileName), accountInfoJson);
        }

        public void SaveWithdraws(WithdrawHistory withdrawHistory)
        {
            var withdrawJson = withdrawHistory.WithdrawList.OrderBy(t => t.ApplyTime).Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_withdrawFileName), withdrawJson);
        }

        public void SaveDeposits(DepositHistory depositHistory)
        {
            var depositJson = depositHistory.DepositList.OrderBy(t => t.InsertTime).Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_depositFileName), depositJson);
        }

        public void SaveDividends(DividendHistory dividendHistory)
        {
            var lines = dividendHistory.Rows.OrderBy(t => t.DivTime).Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_dividendsFileName), lines);
        }

        public void SaveDusts(DustHistory dustHistory)
        {
            var dustHistoryJson = dustHistory.Results.Rows.Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_dustsFileName), dustHistoryJson);
        }

        public void SaveTradingRules(TradingRules tradingRules)
        {
            var tradingRulesJson = JsonConvert.SerializeObject(tradingRules);
            File.WriteAllText(GetFileName(_tradingRulesFileName), tradingRulesJson);
        }
        
        public StreamWriter GetHistoPricesStreamWriter()
        {
            var fileName = GetFileName(_histoPricesFileName);
            return new StreamWriter(fileName);
        }

        public StreamWriter GetTradesStreamWriter()
        {
            var fileName = GetFileName(_tradesFileName);
            return new StreamWriter(fileName);
        }

        public void Save(ImmutableArray<KuCoinTrade> result)
        {
            var lines = result.Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_kcTradesFileName), lines);
        }

        public void Save(KuCoinDeposits result)
        {
            var lines = result.Items.Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_kcDepositFileName), lines);
        }

        public void Save(KuCoinWithdrawals result)
        {
            var lines = result.Items.Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_kcWithdrawFileName), lines);
        }

        public void Save(ImmutableArray<KcBalance> result)
        {
            var lines = result
                .Select(JsonConvert.SerializeObject);
            File.WriteAllLines(GetFileName(_kcBalancesFileName), lines);
        }

        public void SaveCalculatedPositions(IEnumerable<Position> positions, DateTime date)
        {
            var positionsJson = positions.Select(JsonConvert.SerializeObject);

            var filePath = Path.Combine(SpecialDirectories.MyDocuments, "Taxes", _tag, $"CalculatedPositionsAsOf{date:yyyMMdd}.json");
            File.WriteAllLines(GetFileName(filePath), positionsJson);
        }
        
        private ImmutableArray<ManuallyExtractedTrade> GetManuallyExtractedTrades()
        {
            return Directory.GetFiles(_accountDirectory, ManualExtractTrades)
                .SelectMany(t=>File.ReadAllLines(t).Skip(1))
                .Select(t => new ManuallyExtractedTrade(t))
                .ToImmutableArray();
        }
        
        private ImmutableArray<ITrade> GetBinanceTrades()
        {
            return Directory.GetFiles(_accountDirectory, BinanceTrades)
                // Trades_ is for temporary files
                .Where(t=>!Path.GetFileNameWithoutExtension(t).StartsWith("Trades_"))
                .SelectMany(File.ReadAllLines)
                .Select(JsonConvert.DeserializeObject<Trade>)
                .Select(t => (ITrade) _tradeAbstractedFactory.Invoke(t))
                .ToImmutableArray();
        }

        public static string GetFileName(string fullPath)
        {
            if (!File.Exists(fullPath))
                return fullPath;

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);
            var path = Path.GetDirectoryName(fullPath);

            return Path.Combine(path, $"{fileNameWithoutExtension}_{Guid.NewGuid().ToString().Substring(0, 4)}{Path.GetExtension(fullPath)}");
        }
        
        public static string GetFolder(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                return fullPath;
            }

            var fullPathWithSuffix = $"{fullPath}_{Guid.NewGuid().ToString().Substring(0, 4)}{Path.GetExtension(fullPath)}";
            Directory.CreateDirectory(fullPathWithSuffix);
            return fullPathWithSuffix;
        }
    }
}