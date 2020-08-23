using System;

namespace Common.ModelAbstractions
{
    public interface IWithdraw
    {
        long ApplyTime { get; }
        string Asset { get; }
        decimal Amount { get; }
        decimal TransactionFee { get; }
        DateTime TimeReadable { get; }
    }
}