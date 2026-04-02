
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Shared.Kafka;
using Trade.Bot.Models;
using Trade.Bot.Services;

namespace Trade.Bot.Exchanges;

public class BybitClient : IExchangeClient
{
    private readonly ILogger<BybitClient> _logger;
    private readonly ISymbolCache _cache;
    private readonly Dictionary<string, BybitRestClient> _clients = new();
    private readonly IKafkaProducer _kafkaproduce;
    public BybitClient(ILogger<BybitClient> logger, IKafkaProducer kafkaproduce, ISymbolCache cache)
    {
        _logger = logger;
        _kafkaproduce = kafkaproduce;
        _cache = cache;
    }
    public string Name => "Bybit";



    private BybitRestClient GetClient(AccountConfig acc)
    {
        if (_clients.ContainsKey(acc.AccountId))
            return _clients[acc.AccountId];

        var client = new BybitRestClient(options =>
        {
            options.Environment = Bybit.Net.BybitEnvironment.DemoTrading;
            options.ApiCredentials = new ApiCredentials(acc.ApiKey, acc.SecretKey);
        });
        _clients[acc.AccountId] = client;
        return client;
    }

    public async Task PlaceOrderAsync(AccountConfig acc, ExchangeOrder order, CancellationToken ct)
    {
        var client = GetClient(acc);

        var side = order.Side?.ToLower() switch
        {
            "buy" or "long" => OrderSide.Buy,
            _ => OrderSide.Sell
        };
        var orderType = order.Market?.ToLower() switch
        {
            "limit" => NewOrderType.Limit,
            _ => NewOrderType.Market,
        };
        var result = await client.V5Api.Trading.PlaceOrderAsync(
            Category.Linear,
            order.Symbol,
            side,
            orderType,
            order.Quantity,
            takeProfit: order.TakeProfit,
            clientOrderId: Guid.NewGuid().ToString(),
            stopLoss: order.StopLoss, price: order.Entry, reduceOnly: order.ReduceOnly
        );
        //client.V5Api.Trading.set
        //var result = await client.V5Api.Trading.GetPositionsAsync(Category.Linear,settleAsset: "USDT");
        //var result = await client.V5Api.Trading.CancelOrderAsync(Category.Linear, order.Symbol);

        if (!result.Success)
        {
            _logger.LogError(result.Error?.Message);
            return;
        }
        string market = (orderType == NewOrderType.Market) ? "market" : "limit";
        string strSide = (side == OrderSide.Buy) ? "buy" : "sell";
        //close position
        if (order.ReduceAll)
        {

            string tradeKey = _cache.BuildTradeStatusKey(acc.AccountId, strSide, market, order.Symbol);
            _cache.RemoveTradeStatus(tradeKey);
        }

        var accountTradeHistory = new { account_id = acc.AccountId, 
                                        msg_id = order.MsgId, 
                                        command = JsonConvert.SerializeObject(order),
                                        created_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                        updated_at = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                        created_by = "admin",
                                        updated_by = "admin",
        };
        object jsonObj = new { entity_name = "AccountTradeHistory", entity_value = JsonConvert.SerializeObject(accountTradeHistory) };
        await _kafkaproduce.ProduceAsync<object>("trade.storage", order.MsgId, jsonObj);

        _logger.LogInformation($"[BYBIT:{acc.AccountId}] placed {order.Symbol}");

    }
    public async Task PlaceAsync(AccountConfig acc, OrderIntent intent, CancellationToken ct)
    {
        //var client = GetClient(acc);

        //var side = intent.Side.ToLower() switch
        //{
        //    "buy" or "long" => OrderSide.Buy,
        //    _ => OrderSide.Sell
        //};


        //var res = await client.V5Api.Trading.PlaceOrderAsync(
        //    Category.Linear,
        //    intent.Symbol,
        //    side,
        //    NewOrderType.Market,
        //    intent.Quantity,
        //    reduceOnly: intent.ReduceOnly
        //);

        //if (!res.Success)
        //    throw new Exception(res.Error?.Message);
    }

    public async Task UpdateStopLossAsync(AccountConfig acc, string symbol, decimal stopLoss, CancellationToken ct)
    {
        var client = GetClient(acc);

        var res = await client.V5Api.Trading.SetTradingStopAsync(
            Category.Linear,
            symbol,
            PositionIdx.OneWayMode,
            stopLoss: stopLoss
        );

        if (!res.Success)
            throw new Exception(res.Error?.Message);
    }
}
