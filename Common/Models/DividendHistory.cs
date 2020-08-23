using System.Collections.Generic;

namespace Common.Models
{
#nullable disable
    public class DividendHistory
    {
        public List<Dividend> Rows { get; set; }
        public int Total { get; set; }
    }
}