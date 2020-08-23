using System.Linq;

namespace Common.Delisting
{
    public class DelistingEntry
    {
        public DelistingEntry(string text)
        {
            var strings = text.Split(",");
            Time = long.Parse(strings[0]);
            var split = strings[1].Split(new[] {'=', ' '})
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
            Base = split[1];
            Price = decimal.Parse(split[2]);
            Quote = split[3];
        }

        public long Time { get;  }
        public string Base { get;  }
        public string Quote { get;  }
        public decimal Price { get;  }
        public bool IsBuy => false;
    }
}