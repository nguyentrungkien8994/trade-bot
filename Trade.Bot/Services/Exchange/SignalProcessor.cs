
using Microsoft.Extensions.Logging;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class SignalProcessor
{
    private ILogger<SignalProcessor> _logger;
    private readonly IAccountProvider _accounts;
    private readonly ITradeContextEngine _contextEngine;


    public SignalProcessor(IAccountProvider accounts,
        ILogger<SignalProcessor> logger,
    ITradeContextEngine contextEngine)
    {
        _accounts = accounts;
        _contextEngine = contextEngine;
        _logger = logger;
    }

    public async Task ProcessAsync(TradeSignal signal)
    {
        try
        {
            string owner = signal.owner ?? "";
            _logger.LogInformation($"Process message owner: {owner} trade : {signal.tradeCommands[0].Side} {signal.tradeCommands[0].Symbol}");
            foreach (var account in _accounts.GetAccounts())
            {
                if (!account.Followers.Contains(owner, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                await _contextEngine.HandleAsync(account, signal);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

    }
}
