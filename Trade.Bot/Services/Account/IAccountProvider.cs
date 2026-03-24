
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public interface IAccountProvider
{
    IEnumerable<AccountConfig> GetAccounts();
}
