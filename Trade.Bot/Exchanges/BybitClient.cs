
using Bybit.Net.Clients;
using Bybit.Net.Enums;
using CryptoExchange.Net.Authentication;
using Trade.Bot.Models;

namespace Trade.Bot.Exchanges;

public class BybitClient : IExchangeClient
{
    public string Name => "Bybit";

    private readonly Dictionary<string, BybitRestClient> _clients = new();

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

        var side = order.Side.ToLower() switch
        {
            "buy" or "long" => OrderSide.Buy,
            _ => OrderSide.Sell
        };
        var result = await client.V5Api.Trading.PlaceOrderAsync(
            Category.Linear,
            order.Symbol,
            side,
            NewOrderType.Market,
            order.Quantity,
            takeProfit: order.TakeProfit,
            stopLoss: order.StopLoss, isLeverage: true
        );
        //client.V5Api.Trading.set
        //var result = await client.V5Api.Trading.GetPositionsAsync(Category.Linear,settleAsset: "USDT");
        //var result = await client.V5Api.Trading.CancelOrderAsync(Category.Linear, order.Symbol);

        if (!result.Success)
            throw new Exception(result.Error?.Message);

        Console.WriteLine($"[BYBIT:{acc.AccountId}] {order.Symbol}");
    }
}
