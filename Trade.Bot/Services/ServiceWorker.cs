
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Helper;
//using Shared.Kafka;
using Shared.Redis;
using Trade.Bot.Models;

namespace Trade.Bot.Services;

public class ServiceWorker : BackgroundService
{
    private readonly SignalProcessor _processor;
    private readonly ILogger<ServiceWorker> _logger;
    //private IKafkaConsumer _kafkaConsumer;
    //private IOptions<KafkaOptions> _options;
    private readonly IConfiguration _configuration;
    private readonly IRedisStreamService _redisStreamConsumer;
    public ServiceWorker(SignalProcessor processor,
        //IKafkaConsumer kafkaConsumer,
        //IOptions<KafkaOptions> options,
        IConfiguration configuration,
        IRedisStreamService redisStreamConsumer,
        ILogger<ServiceWorker> logger)
    {
        _processor = processor;
        //_kafkaConsumer = kafkaConsumer;
        //_options = options;
        _redisStreamConsumer = redisStreamConsumer;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            //await _kafkaConsumer.ConsumeAsync(_options.Value.Topic, HandleMessage, ct);
            string stream = ConfigHelper.GetConfigByKey("REDIS_STREAM", _configuration);
            string group = ConfigHelper.GetConfigByKey("REDIS_GROUP", _configuration);
            var consumer = Environment.MachineName;
            await _redisStreamConsumer.CreateConsumerGroupAsync(stream, group);
            while (!ct.IsCancellationRequested)
            {
                var messages = await _redisStreamConsumer.ReadGroupAsync<object>(
                stream,
                group,
                consumer);
                if (messages.Count > 0)
                {
                    foreach (var message in messages)
                    {
                        string? strData = message.Data?.ToString();
                        if (string.IsNullOrWhiteSpace(strData)) continue;
                        await _redisStreamConsumer.AckAsync(stream,group,message.Id);
                        await HandleMessage(Guid.NewGuid().ToString(), strData);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }


        ////string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Side\":\"Buy\",\"Entry\":67540.0,\"Risk\":10,\"EntryRange\":null,\"StopLoss\":0,\"TakeProfit\":0,\"Market\":\"limit\"}}";
        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": [{\"Symbol\":\"BTCUSDT\",\"Side\":\"SELL\",\"Entry\":71037.0,\"Risk\":10,\"EntryRange\":null,\"StopLoss\":0,\"TakeProfit\":0,\"Market\":\"market\"}]}";
        ////string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Action\":1,\"Side\":\"SELL\",\"StopLoss\":70000}}";
        ////string json = "{\"Owner\":\"kai\", \"TradeCommand\": {\"Symbol\":\"BTCUSDT\",\"Action\":2,\"Side\":\"SELL\",\"ReducePercent\":50}}";
        //string json = "{\"Owner\":\"kai\", \"TradeCommand\": [{\"Symbol\":\"BTCUSDT\",\"Action\":2,\"Side\":\"SELL\",\"ReducePercent\":100}]}";
        //await HandleMessage(Guid.NewGuid().ToString(), json);
        //while (!ct.IsCancellationRequested)
        //{

        //}
    }
    private async Task HandleMessage(string? key, string value)
    {
        // TODO: xử lý nghiệp vụ tại đây
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return;
            _logger.LogInformation("Receive message: " + key);
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            var signal = JsonConvert.DeserializeObject<TradeSignal>(value,settings);
            if (signal == null) return;
            signal.msgId = key;
            await _processor.ProcessAsync(signal);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}