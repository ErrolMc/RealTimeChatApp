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

        public DatabaseService(CosmosClient cosmosClient, IConfiguration configuration)
        {
            _cosmosClient = cosmosClient;
            var databaseId = configuration["CosmosDb:DatabaseId"] ?? DatabaseId;

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
