using System;
using Common.Models;

namespace Common.ModelAbstractions
{
    public class KuCoinDepositAbstracted : IDeposit
    {
        private readonly KuCoinDeposit _deposit;

        public KuCoinDepositAbstracted(KuCoinDeposit deposit)
        {
            _deposit = deposit;
        }

        public long InsertTime => _deposit.CreatedAt;
        public string Asset => _deposit.Currency;
        public DateTime TimeReadable => InsertTime.ToTimeReadable();
        public decimal Amount => _deposit.Amount;
    }
}