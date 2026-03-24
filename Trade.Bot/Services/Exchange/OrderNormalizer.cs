using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public class OrderNormalizer
    {
        private readonly ISymbolCache _cache;

        public OrderNormalizer(ISymbolCache cache)
        {
            _cache = cache;
        }

        public void Normalize(ExchangeOrder order)
        {
            var rule = _cache.Get(order.Symbol);

            var qty = Math.Floor(order.Quantity / rule.QtyStep) * rule.QtyStep;

            if (qty < rule.MinQty)
                throw new Exception($"Qty too small: {qty}");

            order.Quantity = qty;
        }
    }
}
