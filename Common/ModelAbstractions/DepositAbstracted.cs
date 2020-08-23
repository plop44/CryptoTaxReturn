using System;
using Common.Models;

namespace Common.ModelAbstractions
{
    public class DepositAbstracted : IDeposit
    {
        private readonly Deposit _deposit;

        public DepositAbstracted(Deposit deposit)
        {
            _deposit = deposit;
        }

        public long InsertTime => _deposit.InsertTime;
        public string Asset => _deposit.Asset;
        public DateTime TimeReadable => InsertTime.ToTimeReadable();
        public decimal Amount => _deposit.Amount;
    }
}