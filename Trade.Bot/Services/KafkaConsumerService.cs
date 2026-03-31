
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Helper;
using Shared.Kafka;
using System.Text.Json;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly SignalProcessor _processor;
    private readonly ILogger<KafkaConsumerService> _logger;
    private IKafkaConsumer _kafkaConsumer;
    private IOptions<KafkaOptions> _options;

    public KafkaConsumerService(SignalProcessor processor,
        IKafkaConsumer kafkaConsumer,
        IOptions<KafkaOptions> options,
        ILogger<KafkaConsumerService> logger)
    {
        _processor = processor;
        _kafkaConsumer = kafkaConsumer;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        //try
        //{
        //    await _kafkaConsumer.ConsumeAsync(_options.Value.Topic, HandleMessage, ct);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, ex.Message);
        //}


        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Side\":\"Buy\",\"Entry\":67540.0,\"Risk\":10,\"EntryRange\":null,\"StopLoss\":0,\"TakeProfit\":0,\"Market\":\"limit\"}}";
        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Side\":\"SELL\",\"Entry\":71037.0,\"Risk\":10,\"EntryRange\":null,\"StopLoss\":0,\"TakeProfit\":0,\"Market\":\"market\"}}";
        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Action\":1,\"Side\":\"SELL\",\"StopLoss\":70000}}";
        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Action\":2,\"Side\":\"SELL\",\"ReducePercent\":50}}";
        string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Action\":2,\"Side\":\"SELL\",\"ReducePercent\":100}}";
        await HandleMessage(Guid.NewGuid().ToString(), json);
        while (!ct.IsCancellationRequested)
        {

        }
    }
    private async Task HandleMessage(string? key, string value)
    {
        // TODO: xử lý nghiệp vụ tại đây
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return;
            var signal = JsonSerializer.Deserialize<TradeSignal>(value);
            if (signal == null) return;
            signal.MsgId = key;
            await _processor.ProcessAsync(signal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}