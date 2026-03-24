
using Microsoft.Extensions.Options;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class AccountProvider : IAccountProvider
{
    private readonly List<AccountConfig> _accounts;

    public AccountProvider(IOptions<List<AccountConfig>> options)
    {
        _accounts = options.Value ?? new List<AccountConfig>();
    }

    public IEnumerable<AccountConfig> GetAccounts() => _accounts;
}
