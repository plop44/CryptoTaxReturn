using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common.Models
{
    public class AudUsdFxRate
    {
        //source https://www.rba.gov.au/statistics/historical-data.html

        private readonly Dictionary<DateTime, decimal> _fx = new Dictionary<DateTime, decimal>();

        public long LastDate => _fx.Keys.Max(t => t.ToUnixDateTime());

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

        private decimal GetRate(long time)
        {
            var date = time.ToTimeReadable().Date;

            if (_fx.TryGetValue(date, out var result))
                return result;
            if (_fx.TryGetValue(date.AddDays(1), out var result2))
                return result2;
            if (_fx.TryGetValue(date.AddDays(-1), out var result3))
                return result3;
            if (_fx.TryGetValue(date.AddDays(2), out var result4))
                return result4;
            if (_fx.TryGetValue(date.AddDays(-2), out var result5))
                return result5;

            throw  new Exception($"FxRate not found for {date}");
        }

        public decimal GetMultiplierRateFromUsdToAud(long time)
        {
            //  rounding does not affect the final result and the file is readable
            return 1 / GetRate(time);
        }

        public decimal GetMultiplierRateFromUsdToAudAsAnEstimation(long time)
        {
            //  rounding does not affect the final result and the file is readable
            return 1 / GetMultiplierRateEstimationFromUsdToAud(time);
        }

        private decimal GetMultiplierRateEstimationFromUsdToAud(long time)
        {
            var lastTriedDate = time.ToTimeReadable().Date;
            while (true)
            {
                if (_fx.TryGetValue(lastTriedDate, out var result)) return result;
                lastTriedDate = lastTriedDate.AddDays(-1);
            }
        }
    }
}