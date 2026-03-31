using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public interface ISymbolCache
    {
        Task InitializeAsync();
        Task InitializePositionsAsync();
        SymbolRule Get(string symbol);
        bool TryGetTradeStatus(string tradeKey, out PositionState state);
        void UpsertTradeStatus(string tradeKey, PositionState state);
        void RemoveTradeStatus(string tradeKey);
        string BuildTradeStatusKey(string accId, string side,string market, string symbol);
    }
}
