namespace Common.Models
{
    public static class BalanceExtensions
    {
        public static decimal GetTotal(this Balance balance) => balance.Free + balance.Locked;
    }
}