using Bybit.Net.Clients;
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
        private readonly IAccountProvider _accountProvider;

        public BybitStreamService(IBalanceService balanceService, IAccountProvider accountProvider)
        {
            _balanceService = balanceService;
            _accountProvider = accountProvider;
        }

        public async Task StartAsync()
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

                                _balanceService.Update(acc.AccountId, balance);

                                Console.WriteLine($"[BALANCE WS] {acc.AccountId}: {balance}");
                            }
                        }
                    }
                });
            }
        }
    }
}
