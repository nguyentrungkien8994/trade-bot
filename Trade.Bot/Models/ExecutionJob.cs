
using Trade.Bot.Enum;

namespace Trade.Bot.Models;

public class ExecutionJob
{
    public AccountConfig Account { get; set; } = default!;
    public ExchangeOrder Order { get; set; } = default!;
    public string IdempotencyKey { get; set; } = default!;
    public TradeAction Action { get; set; }

    //public string Symbol { get; set; } = default!;
    //public string Side { get; set; } = default!;

    //public decimal Quantity { get; set; }

    //public decimal? StopLoss { get; set; }

    //public bool ReduceOnly { get; set; }

    //public string IdempotencyKey { get; set; } = default!;
}
