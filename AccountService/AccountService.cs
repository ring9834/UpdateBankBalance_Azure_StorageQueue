using Microsoft.Data.SqlClient;
using Dapper;
using QueueService;
using DataModels;

namespace AccountServiceN
{
    public class AccountService
    {
        private readonly string _connectionString;
        private readonly BankQueueService _queueService;

        public AccountService(string dbConnectionString, string storageConnectionString)
        {
            _connectionString = dbConnectionString;
            _queueService = new BankQueueService(storageConnectionString);
        }

        public async Task ProcessDailyUpdatesAsync()
        {
            Console.WriteLine("Starting daily account balance processing...");

            // Process messages in batches
            int processedCount = 0;
            int batchSize = 20;

            while (true)
            {
                var messages = await _queueService.ReceiveMessagesAsync(batchSize);
                if (messages.Count == 0) break;

                foreach (var message in messages)
                {
                    try
                    {
                        await ProcessSingleUpdateAsync(message);
                        processedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process message for account {message.AccountNumber}: {ex.Message}");
                        // Could implement dead-letter queue here
                    }
                }
            }

            Console.WriteLine($"Processed {processedCount} account updates.");
        }

        private async Task ProcessSingleUpdateAsync(AccountUpdateMessage message)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Get current balance
                var currentBalance = await connection.QueryFirstOrDefaultAsync<decimal>(
                    "SELECT CurrentBalance FROM Accounts WHERE AccountNumber = @AccountNumber",
                    new { message.AccountNumber },
                    transaction
                );

                decimal newBalance = message.TransactionType switch
                {
                    "DEPOSIT" or "INTEREST" => currentBalance + message.Amount,
                    "WITHDRAWAL" or "FEE" => currentBalance - message.Amount,
                    _ => throw new ArgumentException($"Invalid transaction type: {message.TransactionType}")
                };

                // Update account balance
                await connection.ExecuteAsync(
                    @"UPDATE Accounts 
                  SET CurrentBalance = @NewBalance, 
                      LastUpdated = GETUTCDATE()
                  WHERE AccountNumber = @AccountNumber",
                    new { NewBalance = newBalance, message.AccountNumber },
                    transaction
                );

                // Record transaction
                await connection.ExecuteAsync(
                    @"INSERT INTO Transactions 
                  (TransactionId, AccountNumber, Amount, Type, Description, Timestamp, BalanceAfterTransaction)
                  VALUES (@TransactionId, @AccountNumber, @Amount, @Type, @Description, @Timestamp, @BalanceAfterTransaction)",
                    new
                    {
                        TransactionId = Guid.NewGuid().ToString(),
                        message.AccountNumber,
                        message.Amount,
                        Type = message.TransactionType,
                        message.Description,
                        Timestamp = DateTime.UtcNow,
                        BalanceAfterTransaction = newBalance
                    },
                    transaction
                );

                await transaction.CommitAsync();

                Console.WriteLine($"Processed {message.TransactionType} of {message.Amount} for account {message.AccountNumber}");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ScheduleDailyInterestAsync()
        {
            // Example: Schedule daily interest calculation
            using var connection = new SqlConnection(_connectionString);

            var accounts = await connection.QueryAsync<AccountBalance>(
                "SELECT AccountNumber, CurrentBalance FROM Accounts WHERE AccountType = 'SAVINGS'"
            );

            foreach (var account in accounts)
            {
                decimal dailyInterest = account.CurrentBalance * 0.0001m; // 0.01% daily interest

                var interestMessage = new AccountUpdateMessage
                {
                    AccountNumber = account.AccountNumber,
                    Amount = dailyInterest,
                    TransactionType = "INTEREST",
                    Description = "Daily interest accrual",
                    TransactionDate = DateTime.UtcNow.Date,
                    ReferenceId = $"INTEREST_{DateTime.UtcNow:yyyyMMdd}"
                };

                await _queueService.SendAccountUpdateAsync(interestMessage);
            }
        }
    }
}
