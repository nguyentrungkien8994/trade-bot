
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class SignalProcessor
{
    private readonly RiskService _risk;
    private readonly IAccountProvider _accounts;
    private readonly IBalanceService _balanceService;
    private readonly ExecutionEngineV2 _engine;
    private readonly OrderNormalizer _normalizer;

    public SignalProcessor(RiskService risk, IAccountProvider accounts, ExecutionEngineV2 engine, OrderNormalizer normalizer, IBalanceService balanceService)
    {
        _risk = risk;
        _accounts = accounts;
        _engine = engine;
        _normalizer = normalizer;
        _balanceService = balanceService;
    }

    public async Task ProcessAsync(TradeSignal signal)
    {
        foreach (var account in _accounts.GetAccounts())
        {
            var size = _risk.CalculatePositionSize(signal.TradeCommand, _balanceService.GetBalance(account.AccountId));

            var order = new ExchangeOrder
            {
                Symbol = signal.TradeCommand.Symbol,
                Side = signal.TradeCommand.Side,
                Quantity = size,
                Entry = signal.TradeCommand.Entry,
                StopLoss = signal.TradeCommand.StopLoss,
                TakeProfit = signal.TradeCommand.TakeProfit
            };

            var key = $"{account.AccountId}-{signal.Owner}-{order.Symbol}-{order.Entry}";
            _normalizer.Normalize(order);
            await _engine.EnqueueAsync(new ExecutionJob
            {
                Account = account,
                Order = order,
                IdempotencyKey = key
            });
        }
    }
}
