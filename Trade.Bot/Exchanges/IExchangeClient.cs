
using Trade.Bot.Models;

namespace Trade.Bot.Exchanges;

public interface IExchangeClient
{
    string Name { get; }

    Task PlaceOrderAsync(AccountConfig account, ExchangeOrder order, CancellationToken ct);
}
