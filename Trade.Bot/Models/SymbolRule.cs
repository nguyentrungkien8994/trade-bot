using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trade.Bot.Models
{
    public class SymbolRule
    {
        public string Symbol { get; set; } = default!;
        public decimal QtyStep { get; set; }
        public decimal MinQty { get; set; }
    }
}
