using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public interface ITradeContextEngine
    {
        Task HandleAsync(AccountConfig acc, TradeSignal signal);
    }
}
