
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Shared.Helper;
using System.Text.Json;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class KafkaConsumerService : BackgroundService
{
    private readonly SignalProcessor _processor;
    private readonly IConfiguration _configuration;

    public KafkaConsumerService(SignalProcessor processor, IConfiguration configuration)
    {
        _processor = processor;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        string kafka_bootstrap = ConfigHelper.GetConfigByKey("KAFKA_BOOTSTRAP", _configuration);
        string kafka_group = ConfigHelper.GetConfigByKey("KAFKA_GROUP_ID", _configuration);
        string kafka_topic = ConfigHelper.GetConfigByKey("KAFKA_TOPIC", _configuration);
        var config = new ConsumerConfig
        {
            BootstrapServers = kafka_bootstrap,
            GroupId = kafka_group,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(kafka_topic);

        while (!ct.IsCancellationRequested)
        {
            var result = consumer.Consume(ct);
            var signal = JsonSerializer.Deserialize<TradeSignal>(result.Message.Value);

            if (signal == null) continue;

            await _processor.ProcessAsync(signal);
        }

        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Side\":\"SELL\",\"Entry\":71037.0,\"Risk\":10,\"EntryRange\":null,\"StopLoss\":0,\"TakeProfit\":0,\"Market\":\"market\"}}";
        //var signal = JsonSerializer.Deserialize<TradeSignal>(json);

        //if (signal == null) return;

        //await _processor.ProcessAsync(signal);
        //while (!ct.IsCancellationRequested)
        //{

        //}
    }
}
