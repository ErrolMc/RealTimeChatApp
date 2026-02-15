using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace ChatApp.Backend.Services
{
    public class DatabaseService
    {
        public const string SECRET_LOGIN_KEY = "f&LvzN7e@&J$XHe&F$7jPR@C6dw#RFKm";

        private static readonly string DatabaseId = "chatappdb";

        private static readonly string UsersContainerID = "users";
        private static readonly string MessagesContainerID = "messages";
        private static readonly string ChatThreadsContainerID = "threads";

        private readonly CosmosClient _cosmosClient;
        private readonly Database _database;

        public Container UsersContainer { get; }
        public Container MessagesContainer { get; }
        public Container ChatThreadsContainer { get; }

        public DatabaseService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("cosmos");
            Console.WriteLine($"[CosmosDB] Connection string starts with: {connectionString?[..Math.Min(50, connectionString?.Length ?? 0)]}...");
            var databaseId = configuration["CosmosDb:DatabaseId"] ?? DatabaseId;

            if (!string.IsNullOrEmpty(connectionString))
            {
                // Aspire Azure CosmosDB may inject an endpoint URL (managed identity) or a full connection string
                if (connectionString.StartsWith("AccountEndpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    // Traditional connection string with key
                    _cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
                    {
                        HttpClientFactory = () => new HttpClient(new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        }),
                        ConnectionMode = ConnectionMode.Gateway,
                        LimitToEndpoint = true
                    });
                }
                else
                {
                    // Endpoint URL only — use managed identity (DefaultAzureCredential)
                    Console.WriteLine($"[CosmosDB] Using managed identity for endpoint: {connectionString}");
                    _cosmosClient = new CosmosClient(connectionString, new DefaultAzureCredential(), new CosmosClientOptions
                    {
                        ConnectionMode = ConnectionMode.Gateway
                    });
                }
            }
            else
            {
                var cosmosUri = Environment.GetEnvironmentVariable("services__cosmos__https__0")
                    ?? configuration["ServiceUrls:CosmosDbUri"];
                var cosmosKey = Environment.GetEnvironmentVariable("COSMOS_PRIMARY_KEY")
                    ?? configuration["CosmosDb:PrimaryKey"];
                _cosmosClient = new CosmosClient(cosmosUri, cosmosKey);
            }

            _database = InitializeDatabase(databaseId).GetAwaiter().GetResult();

            UsersContainer = _database.GetContainer(UsersContainerID);
            MessagesContainer = _database.GetContainer(MessagesContainerID);
            ChatThreadsContainer = _database.GetContainer(ChatThreadsContainerID);
        }

        private async Task<Database> InitializeDatabase(string databaseId)
        {
            Database db;
            try
            {
                var dbResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
                db = dbResponse.Database;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Managed identity may not have permission to create databases — assume it exists
                Console.WriteLine($"[CosmosDB] Cannot create database (Forbidden) — assuming '{databaseId}' already exists");
                db = _cosmosClient.GetDatabase(databaseId);
            }

            try
            {
                await db.CreateContainerIfNotExistsAsync(UsersContainerID, "/userid");
                await db.CreateContainerIfNotExistsAsync(MessagesContainerID, "/threadid");
                await db.CreateContainerIfNotExistsAsync(ChatThreadsContainerID, "/id");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("[CosmosDB] Cannot create containers (Forbidden) — assuming they already exist");
            }

            return db;
        }
    }
}
