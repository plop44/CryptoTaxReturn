using System;
using System.Collections.Generic;
using System.IO;

namespace AddingRates
{
    public class AudUsdFxRate
    {
        //source https://www.rba.gov.au/statistics/historical-data.html

        private readonly Dictionary<DateTime, decimal> _fx = new Dictionary<DateTime, decimal>();

        public AudUsdFxRate()
        {
            var readAllLines = File.ReadAllLines("./AudUsd.csv");

            for (int i = 11; i < readAllLines.Length; i++)
            {
                var line = readAllLines[i].Substring(0,25).Split(",");
                
                if (line[0] == string.Empty)
                    continue;

                var dateTime = DateTime.ParseExact(line[0], "dd-MMM-yyyy",null);
                var value = decimal.Parse(line[1]);
                _fx[dateTime] = value;
            }

            var readAllLines2 = File.ReadAllLines("./AudUsd_old.csv");

            for (int i = 4; i < readAllLines2.Length; i++)
            {
                var line = readAllLines2[i].Substring(0,25).Split(",");
                
                if (line[0] == string.Empty || line[1]=="N/A")
                    continue;

                var dateTime = DateTime.ParseExact(line[0], "dd-MMM-yyyy",null);
                var value = decimal.Parse(line[1]);
                _fx[dateTime] = value;
            }
        }

        public decimal GetRate(DateTime tradeTimeReadable)
        {
            if (_fx.TryGetValue(tradeTimeReadable.Date, out var result))
                return result;
            if (_fx.TryGetValue(tradeTimeReadable.Date.AddDays(1), out var result2))
                return result2;
            if (_fx.TryGetValue(tradeTimeReadable.Date.AddDays(-1), out var result3))
                return result3;
            if (_fx.TryGetValue(tradeTimeReadable.Date.AddDays(2), out var result4))
                return result4;
            if (_fx.TryGetValue(tradeTimeReadable.Date.AddDays(-2), out var result5))
                return result5;

            throw  new Exception($"FxRate not found for {tradeTimeReadable.Date}");
        }

        public decimal GetMultiplierRateFromUsdToAud(DateTime tradeTimeReadable)
        {
            return 1 / GetRate(tradeTimeReadable);
        }
    }
}