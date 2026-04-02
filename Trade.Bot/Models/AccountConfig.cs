
namespace Trade.Bot.Models;

public class AccountConfig
{
    public string AccountId { get; set; } = default!;
    public string Exchange { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public decimal InitBalance { get; set; } = default!;
    public string Followers { get; set; } = default!;
}
