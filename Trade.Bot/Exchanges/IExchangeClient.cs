
using Trade.Bot.Models;

namespace Trade.Bot.Exchanges;

public interface IExchangeClient
{
    string Name { get; }

    Task PlaceOrderAsync(AccountConfig account, ExchangeOrder order, CancellationToken ct);
    Task PlaceAsync(AccountConfig acc, OrderIntent intent, CancellationToken ct);
    Task UpdateStopLossAsync(AccountConfig acc, string symbol, decimal stopLoss, CancellationToken ct);
}
