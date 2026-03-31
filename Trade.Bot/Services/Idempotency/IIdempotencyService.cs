
namespace Trade.Bot.Services;

public interface IIdempotencyService
{
    bool IsProcessed(string key);
    void MarkProcessed(string key);
    void RemoveKey(string key);
}
