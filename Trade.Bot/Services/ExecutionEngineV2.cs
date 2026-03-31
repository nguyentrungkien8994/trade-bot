
using System.Threading.Channels;
using Trade.Bot.Enum;
using Trade.Bot.Exchanges;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class ExecutionEngineV2
{
    private readonly Channel<ExecutionJob> _channel;
    private readonly IEnumerable<IExchangeClient> _clients;
    private readonly IIdempotencyService _idempotency;

    public ExecutionEngineV2(IEnumerable<IExchangeClient> clients, IIdempotencyService idempotency)
    {
        _clients = clients;
        _idempotency = idempotency;

        _channel = Channel.CreateBounded<ExecutionJob>(new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    public async ValueTask EnqueueAsync(ExecutionJob job)
    {
        if (_idempotency.IsProcessed(job.IdempotencyKey))
            return;

        await _channel.Writer.WriteAsync(job);
    }

    public void StartWorkers(int workerCountPerExchange = 4)
    {
        var exchanges = _clients.Select(x => x.Name).Distinct();

        foreach (var exchange in exchanges)
        {
            for (int i = 0; i < workerCountPerExchange; i++)
            {
                _ = Task.Run(() => WorkerLoop(exchange));
            }
        }
    }
    private async Task WorkerLoop(string exchangeName)
    {
        var client = _clients.First(x => x.Name == exchangeName);

        await foreach (var job in _channel.Reader.ReadAllAsync())
        {
            if (!job.Account.Exchange.Equals(exchangeName, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                switch (job.Action)
                {
                    case TradeAction.Open:
                    case TradeAction.Reduce:
                        await client.PlaceOrderAsync(job.Account, job.Order, CancellationToken.None);
                        break;

                    case TradeAction.UpdateSL:
                        await client.UpdateStopLossAsync(
                            job.Account,
                            job.Order.Symbol,
                            job.Order.StopLoss,
                            CancellationToken.None);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXEC ERROR] {ex.Message}");
            }
        }
    }
    //private async Task WorkerLoop(string exchangeName)
    //{
    //    var client = _clients.First(x => x.Name == exchangeName);

    //    await foreach (var job in _channel.Reader.ReadAllAsync())
    //    {
    //        if (!job.Account.Exchange.Equals(exchangeName, StringComparison.OrdinalIgnoreCase))
    //            continue;

    //        try
    //        {
    //            await client.PlaceOrderAsync(job.Account, job.Order, CancellationToken.None);
    //            _idempotency.MarkProcessed(job.IdempotencyKey);
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"[ERROR] {ex.Message}");
    //        }
    //    }
    //}
}
