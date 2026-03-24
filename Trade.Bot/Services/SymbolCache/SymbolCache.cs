using Bybit.Net.Clients;
using Confluent.Kafka;
using CryptoExchange.Net.Authentication;
using System.Globalization;
using Trade.Bot.Exchanges;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public class SymbolCache : ISymbolCache
    {
        private readonly Dictionary<string, SymbolRule> _cache = new();
        private BybitRestClient GetClient()
        {
            var client = new BybitRestClient(options =>
            {
                options.Environment = Bybit.Net.BybitEnvironment.DemoTrading;
            });
            return client;
        }
        public async Task InitializeAsync()
        {
            Console.WriteLine("Loading symbol metadata...");
            var client = GetClient();
            var result = await client.V5Api.ExchangeData.GetLinearInverseSymbolsAsync(Bybit.Net.Enums.Category.Linear);

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

        public SymbolRule Get(string symbol)
        {

            if (!_cache.ContainsKey(symbol))
                throw new Exception($"Symbol not found: {symbol}");

            return _cache[symbol];
        }
    }
}
