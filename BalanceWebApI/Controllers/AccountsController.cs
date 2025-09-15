using AccountServiceN;
using DataModels;
using Microsoft.AspNetCore.Mvc;
using QueueService;

namespace BalanceWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _accountService;
        private readonly BankQueueService _queueService;

        public AccountsController(AccountService accountService, BankQueueService queueService)
        {
            _accountService = accountService;
            _queueService = queueService;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequest request)
        {
            var message = new AccountUpdateMessage
            {
                AccountNumber = request.AccountNumber,
                Amount = request.Amount,
                TransactionType = "DEPOSIT",
                Description = request.Description,
                TransactionDate = DateTime.UtcNow,
                ReferenceId = Guid.NewGuid().ToString()
            };

            var messageId = await _queueService.SendAccountUpdateAsync(message);

            return Ok(new { MessageId = messageId, Status = "Queued" });
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
        {
            var message = new AccountUpdateMessage
            {
                AccountNumber = request.AccountNumber,
                Amount = request.Amount,
                TransactionType = "WITHDRAWAL",
                Description = request.Description,
                TransactionDate = DateTime.UtcNow,
                ReferenceId = Guid.NewGuid().ToString()
            };

            var messageId = await _queueService.SendAccountUpdateAsync(message);

            return Ok(new { MessageId = messageId, Status = "Queued" });
        }

        [HttpPost("process-daily")]
        public async Task<IActionResult> ProcessDailyUpdates()
        {
            await _accountService.ProcessDailyUpdatesAsync();
            return Ok(new { Status = "Processing completed" });
        }

        [HttpGet("queue-status")]
        public async Task<IActionResult> GetQueueStatus()
        {
            var messageCount = await _queueService.GetQueueMessageCountAsync();
            return Ok(new { PendingMessages = messageCount });
        }
    }
}
