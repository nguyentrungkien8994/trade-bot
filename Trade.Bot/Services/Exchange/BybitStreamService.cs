using Bybit.Net.Clients;
using Bybit.Net.Objects.Models.V5;
using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trade.Bot.Models;

namespace Trade.Bot.Services
{
    public class BybitStreamService
    {
        private readonly IBalanceService _balanceService;
        private readonly ISymbolCache _cache;
        private readonly IAccountProvider _accountProvider;

        public BybitStreamService(IBalanceService balanceService,
            IAccountProvider accountProvider,
            ISymbolCache cache)
        {
            _balanceService = balanceService;
            _accountProvider = accountProvider;
            _cache = cache;
        }

        public async Task StartPositionAsync()
        {
            foreach (var acc in _accountProvider.GetAccounts())
            {
                var socket = new BybitSocketClient(options =>
                {
                    options.ApiCredentials = new ApiCredentials(acc.ApiKey, acc.SecretKey);
                    options.OutputOriginalData = true;
                    options.Environment = Bybit.Net.BybitEnvironment.DemoTrading;
                });
                await socket.V5PrivateApi.SubscribeToPositionUpdatesAsync(async data =>
                {
                    foreach (var p in data.Data)
                    {
                        if (p.Side != null)
                        {
                            string side = p.Side == Bybit.Net.Enums.PositionSide.Sell ? "sell" : "buy";
                            string tradeKey = _cache.BuildTradeStatusKey(acc.AccountId, side, "market", p.Symbol);
                            _cache.UpsertTradeStatus(tradeKey, new PositionState() { Side = side, Size = p.Quantity, Symbol = p.Symbol, Entry = (p.AveragePrice??0) });
                        }
                        else
                        {
                            await _cache.InitializePositionsAsync();
                        }
                    }
                });
                //await socket.V5PrivateApi.SubscribeToOrderUpdatesAsync(data =>
                //{
                //    foreach (var p in data.Data)
                //    {
                //        //UpsertPosition(acc, p);
                //        //if (p.Side != null)
                //        //{
                //        //    string tradeKey = _cache.BuildTradeStatusKey(acc.AccountId, p.Side.ToString(), p.Symbol);
                //        //    _cache.UpsertTradeStatus(tradeKey, new PositionState() { Side = p.Side.ToString(), Size = p.Quantity, Symbol = p.Symbol });
                //        //}
                //        //else
                //        //{
                //        //    string tradeKey = _cache.BuildTradeStatusKey(acc.AccountId, "", p.Symbol);
                //        //    _cache.RemoveTradeStatus(tradeKey);
                //        //}
                //    }
                //});
            }
        }

        public async Task StartBalanceAsync()
        {
            foreach (var acc in _accountProvider.GetAccounts())
            {
                var socket = new BybitSocketClient(options =>
                {
                    options.ApiCredentials = new ApiCredentials(acc.ApiKey, acc.SecretKey);
                });

                await socket.V5PrivateApi.SubscribeToWalletUpdatesAsync(data =>
                {
                    foreach (var wallet in data.Data)
                    {
                        foreach (var coin in wallet.Assets)
                        {
                            if (coin.Asset.Equals("USDT", StringComparison.OrdinalIgnoreCase))
                            {
                                var balance = coin.WalletBalance ?? 0;

                                _balanceService.Update(acc.AccountId, 1000);

                                Console.WriteLine($"[BALANCE WS] {acc.AccountId}: {balance}");
                            }
                        }
                    }
                });
            }
        }
    }
}
