
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class RiskService
{
    public decimal CalculatePositionSize(TradeCommand cmd, decimal balance)
    {
    //    if (cmd.Size > 0)
    //        return cmd.Size;
        decimal risk = cmd.Risk;
        if (risk == 0)
            risk = 5;
        var riskAmount = balance * risk / 100;
        var stopDistance = Math.Abs(cmd.Entry - cmd.StopLoss);
        if (stopDistance == 0) return 0;
        return riskAmount / stopDistance;
    }
}
