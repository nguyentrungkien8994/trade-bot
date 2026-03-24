namespace Trade.Bot.Services
{
    public interface IBalanceService
    {
        decimal GetBalance(string accountId);

        bool TryGetBalance(string accountId, out decimal balance);

        void SetInitial();

        void Update(string accountId, decimal balance);

        bool IsReady(string accountId);

        IReadOnlyDictionary<string, decimal> GetAll();

        DateTime? GetLastUpdated(string accountId);

        bool IsStale(string accountId, TimeSpan threshold);
    }
}
