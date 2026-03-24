
namespace Trade.Bot.Models;

public class ExecutionJob
{
    public AccountConfig Account { get; set; } = default!;
    public ExchangeOrder Order { get; set; } = default!;
    public string IdempotencyKey { get; set; } = default!;
}
