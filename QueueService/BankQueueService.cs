using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using DataModels;
using System.Text.Json;

namespace QueueService
{
    public class BankQueueService
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly string _queueName = "bank-account-updates";
        private QueueClient _queueClient;

        public BankQueueService(string connectionString)
        {
            _queueServiceClient = new QueueServiceClient(connectionString);
            InitializeQueueAsync().Wait();
        }

        private async Task InitializeQueueAsync()
        {
            _queueClient = _queueServiceClient.GetQueueClient(_queueName);
            await _queueClient.CreateIfNotExistsAsync();
        }

        public async Task<string> SendAccountUpdateAsync(AccountUpdateMessage message)
        {
            try
            {
                string messageJson = JsonSerializer.Serialize(message);
                var response = await _queueClient.SendMessageAsync(messageJson);
                return response.Value.MessageId;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send message to queue", ex);
            }
        }

        public async Task<List<AccountUpdateMessage>> ReceiveMessagesAsync(int maxMessages = 10)
        {
            var messages = new List<AccountUpdateMessage>();

            QueueMessage[] retrievedMessages = await _queueClient.ReceiveMessagesAsync(maxMessages);

            foreach (QueueMessage message in retrievedMessages)
            {
                try
                {
                    var accountUpdate = JsonSerializer.Deserialize<AccountUpdateMessage>(message.MessageText);
                    messages.Add(accountUpdate);

                    // Delete the message after processing
                    await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }
                catch (JsonException ex)
                {
                    // Handle invalid messages
                    Console.WriteLine($"Invalid message format: {ex.Message}");
                    await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }
            }

            return messages;
        }

        public async Task<int> GetQueueMessageCountAsync()
        {
            QueueProperties properties = await _queueClient.GetPropertiesAsync();
            return properties.ApproximateMessagesCount;
        }
    }
}
