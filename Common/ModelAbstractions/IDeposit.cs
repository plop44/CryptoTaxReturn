using System;

namespace Common.ModelAbstractions
{
    public interface IDeposit
    {
        long InsertTime { get; }
        string Asset { get; }
        DateTime TimeReadable { get; }
        decimal Amount { get; }
    }
}
