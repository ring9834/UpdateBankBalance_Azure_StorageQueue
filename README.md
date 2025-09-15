# Update User Account Balance uing Azure Storage Queue
This code is about how to use Azure Storage Queue as messaging service to implement asynchronously handling daily account balance updates (calculating interest, applying fees, or reconciling transactions) after deposit and withdraw to ensure scalability and responsiveness. Azure Storage Queue supports transactions ideal for high-throughput (20,000 messages/sec per queue), basic queuing needs without requiring advanced messaging features like strict ordering or transactions.

## Technologies used
Azure Storage Queque (C#)

Dapper (a high-performance ORM framework)

RESTful API

Background Worker
