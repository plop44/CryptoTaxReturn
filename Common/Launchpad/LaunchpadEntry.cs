using System;
using Common.Models;

namespace Common.Launchpad
{
    public class LaunchpadEntry
    {
        public LaunchpadEntry(string row)
        {
            var strings = row.Split(",");
            Asset = strings[0];
            ProjectName = strings[1];
            Price = decimal.Parse(strings[2]);
            DividendTimeApproximationTimeReadable = DateTime.Parse(strings[3]);
        }

        public string Asset { get; }
        public string ProjectName { get; }
        public decimal Price { get; }
        public long DividendTimeApproximation => DividendTimeApproximationTimeReadable.ToUnixDateTime();
        public DateTime DividendTimeApproximationTimeReadable { get; }
    }
}