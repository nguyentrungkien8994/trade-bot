
namespace Trade.Bot.Models;

public class TradeSignal
{
    public string Owner { get; set; } = default!;
    public TradeCommand TradeCommand { get; set; } = default!;
}

public class TradeCommand
{
    public string Symbol { get; set; } = default!;
    public string Market { get; set; } = default!;
    public string Side { get; set; } = default!;
    public decimal Risk { get; set; }
    public decimal Size { get; set; }
    public decimal Entry { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
}
