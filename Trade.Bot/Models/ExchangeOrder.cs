
namespace Trade.Bot.Models;

public class ExchangeOrder
{
    public string Symbol { get; set; } = default!;
    public string Side { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal Entry { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
}
