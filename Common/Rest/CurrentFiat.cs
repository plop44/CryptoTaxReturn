using System;

namespace Common.Rest
{
    public sealed class CurrentFiat
    {
        public CurrentFiat(ICurrentFiatConfig config)
        {
            var configCurrency = config.Currency;

            if (configCurrency != "USD" && configCurrency != "AUD")
                throw new ArgumentException($"Currency {configCurrency} not implemented yet.");
            Name = configCurrency;
        }

        public string Name { get; }
    }
}