using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
//using Shared.Kafka;
using Shared.Logger;

namespace Trade.Bot;

public static class DependencyInjectExtension
{
    /// <summary>
    /// add confluent kafka connection
    /// </summary>
    /// <param name="services"></param>
    //public static void AddConfluentKafka(this IServiceCollection services, string bootstrap,string groupId, string topic)
    //{
    //    // Kafka options
    //    services.AddSingleton<IOptions<KafkaOptions>>(
    //        Options.Create(new KafkaOptions
    //        {
    //            BootstrapServers = bootstrap,
    //            GroupId = groupId,
    //            Topic = topic
    //        })
    //    );
    //    services.AddSingleton<IKafkaConsumer, KafkaConsumer>();
    //    services.AddSingleton<IKafkaProducer, KafkaProducer>();
    //}

    /// <summary>
    /// add nlog
    /// </summary>
    /// <param name="services"></param>
    public static void AddNLog(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddNLog(NLogConfiguration.GetConfig());
        });

    }
    /// <summary>
    /// add redis
    /// </summary>
    /// <param name="services"></param>
    public static void AddExchangeRedis(this IServiceCollection services, string redisServer, string redisInstance)
    {
        services.AddRedis(options =>
        {
            options.ConnectionString = redisServer;
            options.InstanceName = redisInstance;
        });
    }
}
