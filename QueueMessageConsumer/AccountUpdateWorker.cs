using AccountServiceN;
using Microsoft.Extensions.Hosting; // Add this using directive
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueMessageConsumer
{
    public class AccountUpdateWorker : BackgroundService
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountUpdateWorker> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(5);

        public AccountUpdateWorker(AccountService accountService, ILogger<AccountUpdateWorker> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Account Update Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _accountService.ProcessDailyUpdatesAsync();
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing account updates");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait before retry
                }
            }
        }
    }
}
