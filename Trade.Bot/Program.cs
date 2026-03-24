
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Trade.Bot.Services;
using Trade.Bot.Models;
using Trade.Bot.Exchanges;

var builder = Host.CreateApplicationBuilder(args);

// bind config
builder.Services.Configure<List<AccountConfig>>(
    builder.Configuration.GetSection("Accounts"));

// core
builder.Services.AddSingleton<IAccountProvider, AccountProvider>();
builder.Services.AddSingleton<IIdempotencyService, InMemoryIdempotencyService>();
builder.Services.AddSingleton<RiskService>();

// engine
builder.Services.AddSingleton<ExecutionEngineV2>();

// exchanges
builder.Services.AddSingleton<IExchangeClient, BybitClient>();
builder.Services.AddSingleton<ISymbolCache, SymbolCache>();
builder.Services.AddSingleton<IBalanceService, BalanceService>();
builder.Services.AddSingleton<OrderNormalizer>();
//builder.Services.AddSingleton<IExchangeClient, BinanceClient>();

// processor
builder.Services.AddSingleton<SignalProcessor>();

builder.Services.AddSingleton<BybitStreamService>();

// background worker
builder.Services.AddHostedService<KafkaConsumerService>();

var app = builder.Build();


var cache = app.Services.GetRequiredService<ISymbolCache>();
await cache.InitializeAsync();
var balanceService = app.Services.GetRequiredService<IBalanceService>();
balanceService.SetInitial();
var streamService = app.Services.GetRequiredService<BybitStreamService>();
await streamService.StartAsync();
// start workers
var engine = app.Services.GetRequiredService<ExecutionEngineV2>();
engine.StartWorkers(workerCountPerExchange: 4);

await app.RunAsync();
