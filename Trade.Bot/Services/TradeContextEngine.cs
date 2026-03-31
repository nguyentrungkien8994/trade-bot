using Microsoft.Extensions.Logging;
using Trade.Bot.Enum;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public class TradeContextEngine : ITradeContextEngine
    {
        private readonly ILogger<TradeContextEngine> _logger;
        private readonly ISymbolCache _cache;
        private readonly IBalanceService _balances;
        private readonly IIdempotencyService _idem;
        private readonly IBalanceService _balanceService;
        private readonly ExecutionEngineV2 _engine;
        private readonly OrderNormalizer _normalizer;
        private readonly RiskService _risk;


        public TradeContextEngine(
            ILogger<TradeContextEngine> logger,
            ISymbolCache cache,
            IBalanceService balances,
            IIdempotencyService idem,
            IBalanceService balanceService,
            ExecutionEngineV2 engine,
            OrderNormalizer normalizer,
            RiskService risk)
        {
            _logger = logger;
            _cache = cache;
            _balances = balances;
            _idem = idem;
            _engine = engine;
            _normalizer = normalizer;
            _balanceService = balanceService;
            _risk = risk;
        }

        public async Task HandleAsync(AccountConfig acc, TradeSignal signal)
        {
            if (signal == null || signal.TradeCommand == null)
            {
                _logger.LogWarning("Signal or TradeCommand null");
                return;
            }
            if (!_balances.IsReady(acc.AccountId))
            {
                _logger.LogWarning("Account not ready");
                return;
            }
            TradeCommand cmd = signal.TradeCommand;

            switch (cmd.Action)
            {
                case TradeAction.Open:
                    await HandleOpen(acc, signal);
                    break;

                case TradeAction.UpdateSL:
                    await HandleSL(acc, signal);
                    break;

                case TradeAction.Reduce:
                    await HandleReduce(acc, signal);
                    break;
            }
        }

        private PositionState GetPositionState(string accId, TradeCommand cmd)
        {
            var market = cmd.Market?.ToLower() switch
            {
                "limit" => "limit",
                _ => "market",
            };
            string tradeKey = _cache.BuildTradeStatusKey(accId, cmd.Side, market, cmd.Symbol);
            _cache.TryGetTradeStatus(tradeKey, out var pos);
            return pos;
        }

        // ===== OPEN =====
        private async Task HandleOpen(AccountConfig acc, TradeSignal signal)
        {
            TradeCommand cmd = signal.TradeCommand;
            var pos = GetPositionState(acc.AccountId, cmd);
            if (pos != null)
                return;

            var size = _risk.CalculatePositionSize(cmd, _balanceService.GetBalance(acc.AccountId));

            var order = new ExchangeOrder
            {
                Symbol = cmd.Symbol,
                Side = cmd.Side,
                Market = cmd.Market,
                Quantity = size,
                Entry = cmd.Entry,
                StopLoss = cmd.StopLoss,
                TakeProfit = cmd.TakeProfit,
                MsgId = signal.MsgId,
            };

            var key = $"{acc.AccountId}_{cmd.Side}_{cmd.Market}{cmd.Symbol}".ToLower();

            _normalizer.Normalize(order);

            var job = new ExecutionJob
            {
                Account = acc,
                Action = TradeAction.Open,
                Order = order,
                IdempotencyKey = key
            };

            await _engine.EnqueueAsync(job);
            //_idem.MarkProcessed(key);
        }

        // ===== UPDATE SL =====
        private async Task HandleSL(AccountConfig acc, TradeSignal signal)
        {
            TradeCommand cmd = signal.TradeCommand;
            var pos = GetPositionState(acc.AccountId, cmd);
            if (pos == null || cmd.StopLoss == 0)
                return;

            var key = $"{acc.AccountId}_SL_{cmd.Symbol}_{cmd.StopLoss}";
            var job = new ExecutionJob
            {
                Account = acc,
                Action = TradeAction.UpdateSL,
                Order = new ExchangeOrder
                {
                    Symbol = cmd.Symbol,
                    StopLoss = cmd.StopLoss,
                    MsgId = signal.MsgId,
                },
                IdempotencyKey = key
            };

            await _engine.EnqueueAsync(job);

            //_idem.MarkProcessed(key);
        }

        // ===== REDUCE =====
        private async Task HandleReduce(AccountConfig acc, TradeSignal signal)
        {
            TradeCommand cmd = signal.TradeCommand;
            var pos = GetPositionState(acc.AccountId, cmd);
            if (pos == null || cmd.ReducePercent == 0)
                return;

            decimal qty = pos.Size * (cmd.ReducePercent / 100);
            if (cmd.ReducePercent > 99)
            {
                qty = pos.Size;
            }
            var side = pos.Side.Equals("buy", StringComparison.OrdinalIgnoreCase) ? "Sell" : "Buy";

            var order = new ExchangeOrder
            {
                Symbol = cmd.Symbol,
                Side = side,
                Market = "",
                Quantity = qty,
                Entry = cmd.Entry,
                StopLoss = cmd.StopLoss,
                TakeProfit = cmd.TakeProfit,
                MsgId = signal.MsgId,
                ReduceOnly = true,
                ReduceAll = (cmd.ReducePercent > 99)
            };
            _normalizer.Normalize(order);
            var key = $"{acc.AccountId}_REDUCE_{cmd.Symbol}_{cmd.ReducePercent}_size_{pos.Size}".ToLower();


            var job = new ExecutionJob
            {
                Account = acc,
                Action = TradeAction.Reduce,
                Order = order,
                IdempotencyKey = key
            };

            await _engine.EnqueueAsync(job);

            //_idem.MarkProcessed(key);
        }
    }
}
