using Bybit.Net.Enums;
namespace Trade.Bot.Models
{
    public class PositionState
    {
        public string Symbol { get; set; } = default!;
        public string Side { get; set; } = default!; // Buy/Sell
        public required decimal Size { get; set; }
        public required decimal Entry { get; set; }
    }
}
