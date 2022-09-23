using Microsoft.Azure.Cosmos;

class ToDoItem{

    public int Id { get; set; }
    public string? Category { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
}

public class Program
{
    // Replace <documentEndpoint> with the information created earlier
    private static readonly string EndpointUri = "<Endpoint Uri>";

    // Set variable to the Primary Key from earlier.
    private static readonly string PrimaryKey = "<Primary key>";

    // The names of the database and container we will create
    const string databaseId = "ToDoList";
    const string containerId = "Items";
    const string partitionKeyPath = "/category";

    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Beginning operations...\n");
            await CosmosAsync();

        }
        catch (CosmosException de)
        {
            Exception baseException = de.GetBaseException();
            Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: {0}", e);
        }
        finally
        {
            Console.WriteLine("End of program, press any key to exit.");
            Console.ReadKey();
        }
    }
    //The sample code below gets added below this line
    static async Task CosmosAsync()
    {
        // Create a new instance of the Cosmos Client
        var cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);

        // Runs the CreateDatabaseAsync method
        var database = await CreateDatabaseAsync(cosmosClient);

        Console.WriteLine("Created Database: {0}\n", database.Id);

        // Run the CreateContainerAsync method
        var container = await CreateContainerAsync(database);
        
        Console.WriteLine("Created Container: {0}\n", container.Id);

        await QueryData(container);

    }    

    static async Task<Database> CreateDatabaseAsync(CosmosClient cosmosClient)
    {
        // Create a new database using the cosmosClient
        return await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
    }

    static async Task<Container> CreateContainerAsync(Database database)
    {
        // Create a new container
        return await database.CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
    }

    static async Task QueryData(Container container){
        QueryDefinition query = new QueryDefinition(
            "select * from Items i where i.category = @Category ")
            .WithParameter("@Category", "personal");

        FeedIterator<ToDoItem> resultSet = container.GetItemQueryIterator<ToDoItem>(
            queryDefinition: query,
            requestOptions: new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey("category")
            });

            // Iterate query result pages
            while (resultSet.HasMoreResults)
            {
                FeedResponse<ToDoItem> response = await resultSet.ReadNextAsync();

                // Iterate query results
                foreach (ToDoItem item in response)
                {
                    Console.WriteLine($"Found item:\t{item.Name}:\t{item.Description}");
                }
            }
    }
}

