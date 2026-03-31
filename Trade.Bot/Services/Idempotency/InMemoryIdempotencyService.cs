
using System.Collections.Concurrent;

namespace Trade.Bot.Services;

public class InMemoryIdempotencyService : IIdempotencyService
{
    private readonly ConcurrentDictionary<string, bool> _store = new();
    public bool IsProcessed(string key) => _store.ContainsKey(key);

    public void MarkProcessed(string key) => _store[key] = true;
    public void RemoveKey(string key)
    {
        _store.Remove(key,out _);
    }
}
