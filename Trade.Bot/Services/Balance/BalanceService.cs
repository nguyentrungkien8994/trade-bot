using System.Collections.Concurrent;

namespace Trade.Bot.Services
{
    public class BalanceService : IBalanceService
    {
        private readonly ConcurrentDictionary<string, decimal> _balances = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastUpdated = new();
        private readonly IAccountProvider _accounts;
        public BalanceService(IAccountProvider accounts)
        {
            _accounts = accounts;
        }
        // ===== READ =====

        public decimal GetBalance(string accountId)
        {
            if (_balances.TryGetValue(accountId, out var balance))
                return balance;

            throw new InvalidOperationException($"Balance not available: {accountId}");
        }

        public bool TryGetBalance(string accountId, out decimal balance)
        {
            return _balances.TryGetValue(accountId, out balance);
        }

        public bool IsReady(string accountId)
        {
            return _balances.ContainsKey(accountId);
        }

        public IReadOnlyDictionary<string, decimal> GetAll()
        {
            return _balances;
        }

        // ===== WRITE =====

        public void SetInitial()
        {
            foreach (var account in _accounts.GetAccounts())
            {
                _balances[account.AccountId] = account.InitBalance;
                _lastUpdated[account.AccountId] = DateTime.UtcNow;
            }
        }

        public void Update(string accountId, decimal balance)
        {
            _balances[accountId] = balance;
            _lastUpdated[accountId] = DateTime.UtcNow;
        }

        // ===== META =====

        public DateTime? GetLastUpdated(string accountId)
        {
            if (_lastUpdated.TryGetValue(accountId, out var time))
                return time;

            return null;
        }

        public bool IsStale(string accountId, TimeSpan threshold)
        {
            if (!_lastUpdated.TryGetValue(accountId, out var time))
                return true;

            return DateTime.UtcNow - time > threshold;
        }
    }
}
