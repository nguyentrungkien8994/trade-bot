using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade.Bot.Enum;

namespace Trade.Bot.Models
{
    public class OrderIntent
    {
        public TradeAction Action { get; set; }
        public string Symbol { get; set; } = default!;
        public string Side { get; set; } = default!;
        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }
        public bool ReduceOnly { get; set; }
    }
}
