using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Common.Delisting;
using Common.Files;
using Common.Launchpad;
using Common.ModelAbstractions;
using Common.Models;
using Common.TradeAbstractions;

namespace Common
{
    public class PortfolioAcrossTime
    {
        private readonly List<CapitalGain> _capitalGains = new List<CapitalGain>();
        private readonly LocalDataRetrieverComposite _fileRepository;
        private readonly Portfolio _portfolio;
        private long _lastEvent;
        private readonly LaunchpadTradeGenerator _launchpadTradeGenerator;
        private readonly DelistingTradeGenerator _delistingTradeGenerator;
        private readonly IPortfolioAcrossTimeConfig _config;
        public event Action<long>? BeforeProcessing;

        public PortfolioAcrossTime(LocalDataRetrieverComposite fileRepository, 
            Portfolio portfolio, LaunchpadTradeGenerator launchpadTradeGenerator, 
            DelistingTradeGenerator delistingTradeGenerator, IPortfolioAcrossTimeConfig config)
        {
            _fileRepository = fileRepository;
            _portfolio = portfolio;
            _launchpadTradeGenerator = launchpadTradeGenerator;
            _delistingTradeGenerator = delistingTradeGenerator;
            _config = config;
        }

        public void Run()
        {
            var trades = _fileRepository.GetTrades();
            var deposits = _fileRepository.GetDeposits();
            var withdrawals = _fileRepository.GetWithdraws();
            var dividends = _fileRepository.GetDividends();
            var dusts = _fileRepository.GetDusts();
            var fundsOrigin = _fileRepository.GetFundsOrigin();

            Console.WriteLine($"{trades.Length:N0} trades, {dividends.Length:N0} dividends, {deposits.Length:N0} deposits, {withdrawals.Length:N0} withdraws, {dusts.Length:N0} dusts");

            var allEventInOrder = trades.Select(t => (t.Time, (object) t))
                .Concat(deposits.Select(t => (t.InsertTime, (object) t)))
                .Concat(withdrawals.Select(t => (applyTime: t.ApplyTime, (object) t)))
                .Concat(dividends.Select(t => (t.DivTime, (object) t)))
                .Concat(dusts.Select(t => (t.OperateTimeLong, (object) t)))
                .Concat(fundsOrigin.Select(t => (t.Time, (object) t)))
                .OrderBy(GetKey)
                .ToArray();

            foreach (var eventTimeAndObject in allEventInOrder)
            {
                BeforeProcessing?.Invoke(eventTimeAndObject.Item1);

                IEnumerable<CapitalGain> capitalGain = Enumerable.Empty<CapitalGain>();
                switch (eventTimeAndObject.Item2)
                {
                    case ITrade trade: capitalGain = _portfolio.Process(trade); break;
                    case IDeposit deposit: _portfolio.Process(deposit); break;
                    case IWithdraw withdraw: capitalGain = _portfolio.Process(withdraw); break;
                    case Dividend dividend:
                    {
                        var launchpadTrade = _launchpadTradeGenerator.TryGetTrade(dividend);
                        if (launchpadTrade != null)
                        {
                            capitalGain = _portfolio.Process(launchpadTrade);
                            break;
                        }
                        var delistingTrade = _delistingTradeGenerator.TryGetTrade(dividend);
                        if (delistingTrade != null)
                        {
                            capitalGain = _portfolio.Process(delistingTrade);
                            break;
                        }
                        
                        _portfolio.Process(dividend); break;
                    }
                    case DustRow dust: capitalGain=_portfolio.Process(dust); break;
                    case FundsOrigin fundOrigin: _portfolio.Process(fundOrigin); break;
                    default: throw new Exception($"Please add type {eventTimeAndObject.Item2.GetType()}");
                }

                _lastEvent = eventTimeAndObject.Item1;
                _capitalGains.AddRange(capitalGain);
            }
        }

        private long GetKey((long, object) input)
        {
            // we want the IsBuy first
            // that is useful when two trades happens at the same time.
            var inputBigger = input.Item1 << 1;

            if (input.Item2 is ITrade trade && !trade.IsBuyer)
                return inputBigger | 1;

            return inputBigger;
        }

        public DateTime LastEvent => DateTimeOffset.FromUnixTimeMilliseconds(_lastEvent).DateTime;

        public ImmutableArray<CapitalGain> GetCapitalGains()
        {
            if (!_config.IsNewTaxCitizenStartDateApplies)
                return _capitalGains.ToImmutableArray();

            var configNewTaxCitizenStartDate = _config.GetNewTaxCitizenStartDateAsLong();

            return _capitalGains.Where(t => t.SoldTime >= configNewTaxCitizenStartDate)
                .Do(t =>
                {
                    if (t.BoughtTime < configNewTaxCitizenStartDate)
                    {
                        t.BoughtTime = configNewTaxCitizenStartDate;
                    }
                })
                .ToImmutableArray();
        }
    }

    public interface IPortfolioAcrossTimeConfig
    {
        bool IsNewTaxCitizenStartDateApplies { get; }
        long GetNewTaxCitizenStartDateAsLong();
    }
}