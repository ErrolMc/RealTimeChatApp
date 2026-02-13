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
            var databaseId = configuration["CosmosDb:DatabaseId"] ?? DatabaseId;

            if (!string.IsNullOrEmpty(connectionString))
            {
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
            var dbResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            var db = dbResponse.Database;

            await db.CreateContainerIfNotExistsAsync(UsersContainerID, "/userid");
            await db.CreateContainerIfNotExistsAsync(MessagesContainerID, "/threadid");
            await db.CreateContainerIfNotExistsAsync(ChatThreadsContainerID, "/id");

            return db;
        }
    }
}
