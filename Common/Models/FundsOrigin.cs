using System;

namespace Common.Models
{
    public class FundsOrigin
    {
        public FundsOrigin(string line)
        {
            var split = line.Split(',');

            if(split.Length<3)
                throw new Exception($"Line not valid {line}");

            Asset = split[0];
            Quantity = decimal.Parse(split[1]);
            Time = long.Parse(split[2]);
        }

        public string Asset { get; }
        public decimal Quantity { get;  }
        public long Time { get;  }
    }
}