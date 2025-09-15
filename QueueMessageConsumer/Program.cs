// See https://aka.ms/new-console-template for more information
using QueueMessageConsumer;

//Console.WriteLine("Hello, World!");

AccountUpdateWorker worker = new AccountUpdateWorker(null, null);
await worker.StartAsync(new CancellationToken());
