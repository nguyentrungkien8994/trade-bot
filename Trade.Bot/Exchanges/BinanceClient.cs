
using Trade.Bot.Models;

namespace Trade.Bot.Exchanges;

public class BinanceClient : IExchangeClient
{
    public string Name => "Binance";

    public Task PlaceAsync(AccountConfig acc, OrderIntent intent, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task PlaceOrderAsync(AccountConfig acc, ExchangeOrder order, CancellationToken ct)
    {
        await Task.Delay(10, ct);
        Console.WriteLine($"[BINANCE:{acc.AccountId}] {order.Symbol}");
    }

    public Task UpdateStopLossAsync(AccountConfig acc, string symbol, decimal stopLoss, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
