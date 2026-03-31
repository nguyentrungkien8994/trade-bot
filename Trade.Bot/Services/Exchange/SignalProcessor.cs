
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class SignalProcessor
{
    
    private readonly IAccountProvider _accounts;
    private readonly ITradeContextEngine _contextEngine;
    

    public SignalProcessor( IAccountProvider accounts,
        ITradeContextEngine contextEngine)
    {
        _accounts = accounts;
        _contextEngine = contextEngine;
    }

    public async Task ProcessAsync(TradeSignal signal)
    {
        foreach (var account in _accounts.GetAccounts())
        {
            await _contextEngine.HandleAsync(account, signal);
            //await _engine.EnqueueAsync(new ExecutionJob
            //{
            //    Account = account,
            //    Order = order,
            //    IdempotencyKey = key
            //});
        }
    }
}
