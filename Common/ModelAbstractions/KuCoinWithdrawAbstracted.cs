using System;
using Common.Models;

namespace Common.ModelAbstractions
{
    public class KuCoinWithdrawAbstracted : IWithdraw
    {
        private readonly KuCoinWithdrawal _withdraw;

        public KuCoinWithdrawAbstracted(KuCoinWithdrawal withdraw)
        {
            _withdraw = withdraw;
        }
        public long ApplyTime => _withdraw.CreatedAt;
        public string Asset => _withdraw.Currency;
        public decimal Amount => _withdraw.Amount;
        public decimal TransactionFee => _withdraw.Fee;
        public DateTime TimeReadable => ApplyTime.ToTimeReadable();
    }
}