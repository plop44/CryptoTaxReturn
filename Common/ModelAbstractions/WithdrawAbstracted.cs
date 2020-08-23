using System;
using Common.Models;

namespace Common.ModelAbstractions
{
    public class WithdrawAbstracted : IWithdraw
    {
        private readonly Withdraw _withdraw;

        public WithdrawAbstracted(Withdraw withdraw)
        {
            _withdraw = withdraw;
        }
        public long ApplyTime => _withdraw.ApplyTime;
        public string Asset => _withdraw.Asset;
        public decimal Amount => _withdraw.Amount;
        public decimal TransactionFee => _withdraw.TransactionFee;
        public DateTime TimeReadable => _withdraw.TimeReadable;
    }
}