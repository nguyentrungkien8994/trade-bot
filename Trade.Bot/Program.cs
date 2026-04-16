
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trade.Bot.Services;
using Trade.Bot.Models;
using Trade.Bot.Exchanges;
using Trade.Bot;
using Shared.Helper;

var builder = Host.CreateApplicationBuilder(args);

// bind config
builder.Services.Configure<List<AccountConfig>>(
    builder.Configuration.GetSection("Accounts"));
string kafka_bootstrap = ConfigHelper.GetConfigByKey("KAFKA_BOOTSTRAP", builder.Configuration);
string kafka_group = ConfigHelper.GetConfigByKey("KAFKA_GROUP_ID", builder.Configuration);
string kafka_topic = ConfigHelper.GetConfigByKey("KAFKA_TOPIC", builder.Configuration);

string redisServer = ConfigHelper.GetConfigByKey("REDIS_SERVER", builder.Configuration);
string redisInstance = ConfigHelper.GetConfigByKey("REDIS_INSTANCE", builder.Configuration);
//log
builder.Services.AddNLog();
//builder.Services.AddConfluentKafka(kafka_bootstrap,kafka_group,kafka_topic);
builder.Services.AddExchangeRedis(redisServer,redisInstance);
// core
builder.Services.AddSingleton<IAccountProvider, AccountProvider>();
builder.Services.AddSingleton<IIdempotencyService, InMemoryIdempotencyService>();
// exchanges
builder.Services.AddSingleton<IExchangeClient, BybitClient>();
builder.Services.AddSingleton<Trade.Bot.Services.ISymbolCache, SymbolCache>();
builder.Services.AddSingleton<IBalanceService, BalanceService>();
builder.Services.AddSingleton<ITradeContextEngine, TradeContextEngine>();

// engine
builder.Services.AddSingleton<ExecutionEngineV2>();
//Risk service
builder.Services.AddSingleton<RiskService>();
//order nomalizer
builder.Services.AddSingleton<OrderNormalizer>();
// processor
builder.Services.AddSingleton<SignalProcessor>();
//bybit stream service
builder.Services.AddSingleton<BybitStreamService>();
// background worker
builder.Services.AddHostedService<ServiceWorker>();

var app = builder.Build();

var cache = app.Services.GetRequiredService<ISymbolCache>();
await cache.InitializeAsync();
await cache.InitializePositionsAsync();

var balanceService = app.Services.GetRequiredService<IBalanceService>();
balanceService.SetInitial();

var streamService = app.Services.GetRequiredService<BybitStreamService>();
await streamService.StartBalanceAsync();
await streamService.StartPositionAsync();
//await streamService.StartOrderAsync();
//await streamService.StartUserTradeAsync();
// start workers
var engine = app.Services.GetRequiredService<ExecutionEngineV2>();
engine.StartWorkers(workerCountPerExchange: 4);

await app.RunAsync();
