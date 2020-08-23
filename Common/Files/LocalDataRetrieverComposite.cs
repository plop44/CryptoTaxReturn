using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Common.ModelAbstractions;
using Common.Models;
using Common.Rest;
using Common.TradeAbstractions;

namespace Common.Files
{
    public class LocalDataRetrieverComposite
    {
        private readonly ImmutableArray<AccountFileRepository> _localDataRetrievers;

        public LocalDataRetrieverComposite(Func<ManuallySetFileRepositoryConfig, AccountFileRepository> localDataRetrieverFactory, ILocalDataRetrieverCompositeConfig config, PathsConfig pathsConfig)
        {
            var accountConfigs = config.Account.Split(',').Select(t => t.Trim()).ToHashSet();

            bool ShouldBeIncluded(string fullPath)
            {
                if (accountConfigs.Count == 0 || accountConfigs.Count == 1 && "all".Equals(accountConfigs.Single(), StringComparison.InvariantCultureIgnoreCase)) return true;

                var accountDirectoryName = Path.GetFileName(fullPath);
                return accountConfigs.Contains(accountDirectoryName);
            }

            _localDataRetrievers = Directory.GetDirectories(pathsConfig.ExtractFolder)
                    .Where(t => !t.EndsWith(AccountFileRepository.CommonFolderName))
                .Where(ShouldBeIncluded)
                .Select(t => new ManuallySetFileRepositoryConfig(Path.GetFileName(t)))
                .Select(localDataRetrieverFactory.Invoke)
                .ToImmutableArray();
        }

        public string Account => _localDataRetrievers.Select(t => t.Account).JoinString(",");

        public ImmutableArray<IWithdraw> GetWithdraws()
        {
            return _localDataRetrievers.SelectMany(t => t.GetWithdraws()).ToImmutableArray();
        }

        public ImmutableArray<IDeposit> GetDeposits()
        {
            return _localDataRetrievers.SelectMany(t => t.GetDeposits()).ToImmutableArray();
        }

        public ImmutableArray<Dividend> GetDividends()
        {
            return _localDataRetrievers.SelectMany(t => t.GetDividends()).ToImmutableArray();
        }

        public ImmutableArray<DustRow> GetDusts()
        {
            return _localDataRetrievers.SelectMany(t => t.GetDusts()).ToImmutableArray();
        }

        public ImmutableArray<ITrade> GetTrades()
        {
            return _localDataRetrievers.SelectMany(t => t.GetTrades()).ToImmutableArray();
        }

        public ImmutableArray<HistoSymbolPrice> GetHistoPrices()
        {
            return _localDataRetrievers.SelectMany(t => t.GetHistoPrices()).ToImmutableArray();
        }

        public TradingRules GetTradingRules()
        {
            return _localDataRetrievers.First().GetTradingRules();
        }

        public ImmutableArray<Balance> GetAccountInfoBalances()
        {
            return _localDataRetrievers.SelectMany(t => t.GetAccountInfoBalances()).GroupBy(t => t.Asset)
                .Select(t => new Balance {Asset = t.Key, Free = t.Sum(t2 => t2.Free), Locked = t.Sum(t2 => t2.Locked)})
                .ToImmutableArray();
        }

        public ImmutableArray<FundsOrigin> GetFundsOrigin()
        {
            return _localDataRetrievers.First().GetFundsOrigin();
        }

        public class ManuallySetFileRepositoryConfig : IFileRepositoryConfig
        {
            public ManuallySetFileRepositoryConfig(string account)
            {
                Account = account;
            }

            public string Account { get; }
        }
    }

    public interface ILocalDataRetrieverCompositeConfig
    {
        string Account { get;  }
    }
}