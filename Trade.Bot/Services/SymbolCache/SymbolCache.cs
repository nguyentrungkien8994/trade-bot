using Bybit.Net.Clients;
using CryptoExchange.Net.Authentication;
using System.Collections.Concurrent;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public class SymbolCache : ISymbolCache
    {
        private readonly ConcurrentDictionary<string, PositionState> _store = new();
        private readonly Dictionary<string, SymbolRule> _cache = new();
        private readonly IAccountProvider _accountProvider;
        public SymbolCache(IAccountProvider accountProvider)
        {
            _accountProvider = accountProvider;
        }
        private BybitRestClient GetClient(AccountConfig? acc)
        {
            if (acc != null)
            {

                return new BybitRestClient(options =>
                {
                    options.ApiCredentials = new ApiCredentials(acc.ApiKey, acc.SecretKey);
                    options.Environment = Bybit.Net.BybitEnvironment.DemoTrading;
                });
            }
            else
            {
                return new BybitRestClient(options =>
                {
                    options.Environment = Bybit.Net.BybitEnvironment.DemoTrading;
                });
            }
        }
        public async Task InitializeAsync()
        {
            Console.WriteLine("Loading symbol metadata...");
            var client = GetClient(null);
            var result = await client.V5Api.ExchangeData.GetLinearInverseSymbolsAsync(Bybit.Net.Enums.Category.Linear,limit:1000);
            if (!result.Success)
                throw new Exception(result.Error?.Message);

            foreach (var s in result.Data.List)
            {
                var rule = new SymbolRule
                {
                    Symbol = s.Name,
                    QtyStep = s.LotSizeFilter.QuantityStep,
                    MinQty = s.LotSizeFilter.MinOrderQuantity
                };

                _cache[s.Name] = rule;
            }

            Console.WriteLine($"Loaded {_cache.Count} symbols");
        }

        public async Task InitializePositionsAsync()
        {
            Console.WriteLine("Loading current positions...");
            _store.Clear();
            foreach (var acc in _accountProvider.GetAccounts())
            {
                var client = GetClient(acc);
                var result = await client.V5Api.Trading.GetPositionsAsync(Bybit.Net.Enums.Category.Linear, settleAsset: "USDT");
                
                if (!result.Success)
                    throw new Exception(result.Error?.Message);
                
                foreach (var s in result.Data.List)
                {
                    UpsertTradeStatus(acc.AccountId, s.Side == Bybit.Net.Enums.PositionSide.Buy ? "buy" : "sell", s.Symbol, s.Quantity,(s.AveragePrice??0));
                }
            }
            Console.WriteLine("Loaded positions!");
        }

        //private async Task InitializeOrderAsync()
        //{
        //    Console.WriteLine("Loading current orders...");
        //    foreach (var acc in _accountProvider.GetAccounts())
        //    {
        //        var client = GetClient(acc);
        //        var result = await client.V5Api.Trading.GetOrdersAsync(Bybit.Net.Enums.Category.Linear, settleAsset: "USDT");

        //        if (!result.Success)
        //            throw new Exception(result.Error?.Message);

        //        foreach (var s in result.Data.List)
        //        {
        //            UpsertTradeStatus(acc.AccountId, s.Side.ToString(),s.Symbol,s.Quantity);
        //        }
        //    }
        //    Console.WriteLine("Loaded orders!");
        //}
        public void UpsertTradeStatus(string accId, string side, string symbol, decimal size,decimal Entry)
        {
            var position = new PositionState
            {
                Symbol = symbol,
                Side = side,
                Size = size,
                Entry = Entry
            };
            string tradeKey = BuildTradeStatusKey(accId, side, "market", symbol);
            UpsertTradeStatus(tradeKey, position);
            Console.WriteLine($"[Account]: {tradeKey}");
        }
        public SymbolRule Get(string symbol)
        {

            if (!_cache.ContainsKey(symbol))
                throw new Exception($"Symbol not found: {symbol}");

            return _cache[symbol];
        }
        public string BuildTradeStatusKey(string accId, string side, string market, string symbol)
        {
            return $"{accId}_{side}_{market}_{symbol}".ToLower();
        }
        public bool TryGetTradeStatus(string tradeKey, out PositionState state)
        {
            return _store.TryGetValue(tradeKey, out state!);
        }

        public void UpsertTradeStatus(string tradeKey, PositionState state)
        {
            _store[tradeKey] = state;
        }

        public void RemoveTradeStatus(string tradeKey)
        {
            {
                _store.TryRemove(tradeKey, out _);
                Console.WriteLine("Remove position: " + tradeKey);
            }
        }

    }
}
