using System.Threading.Tasks;    
using Azure.Messaging.ServiceBus;


await Send();
Console.WriteLine("Press any key to start receiving");
Console.ReadKey();

await Receive();


// name of your Service Bus topic
const string queueName = "az204-queue";

static async Task Send()
{
    // number of messages to be sent to the queue
    const int numOfMessages = 3;
    // connection string to your Service Bus namespace with Send Permission
    const string connectionString = "<your connection string here>";

    // Create the clients that we'll use for sending and processing messages.
    ServiceBusClient client = new ServiceBusClient(connectionString);
    ServiceBusSender sender = client.CreateSender(queueName);

    // create a batch 
    using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

    for (int i = 1; i <= numOfMessages; i++)
    {
        // try adding a message to the batch
        if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
        {
            // if an exception occurs
            throw new Exception($"Exception {i} has occurred.");
        }
    }

    try 
    {
        // Use the producer client to send the batch of messages to the Service Bus queue
        await sender.SendMessagesAsync(messageBatch);
        Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
    }
    finally
    {
        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await sender.DisposeAsync();
        await client.DisposeAsync();
    }
}

static async Task Receive()
{
    // connection string to your Service Bus namespace with Listen permission
    const  string connectionString = "<your connection string here>";

    // Create the client object that will be used to create sender and receiver objects
    ServiceBusClient client = new ServiceBusClient(connectionString);

    // create a processor that we can use to process the messages
    ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

    try
    {
        // add handler to process messages
        processor.ProcessMessageAsync += async (args) => {
            // handle the message
            Console.WriteLine($"Received: {args.Message.Body.ToString()}");

            // complete the message. messages is deleted from the queue. 
            await args.CompleteMessageAsync(args.Message);
        };

        // add handler to process any errors
        processor.ProcessErrorAsync += (args) => Console.Error.WriteLineAsync(args.Exception.ToString());

        // start processing 
        await processor.StartProcessingAsync();

        Console.WriteLine("Press any key to end the application");
        Console.ReadKey();

        // stop processing 
        Console.WriteLine("\nStopping the receiver...");
        await processor.StopProcessingAsync();
        Console.WriteLine("Stopped receiving messages");
    }
    finally
    {
        // Calling DisposeAsync on client types is required to ensure that network
        // resources and other unmanaged objects are properly cleaned up.
        await processor.DisposeAsync();
        await client.DisposeAsync();
    }
}