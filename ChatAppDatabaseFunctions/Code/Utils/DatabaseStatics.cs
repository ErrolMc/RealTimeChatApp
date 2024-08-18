using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Shared.Constants;

namespace ChatAppDatabaseFunctions.Code
{
    public static class DatabaseStatics
    {
        private static readonly string PrimaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static readonly string DatabaseId = "chatappdb";

        private static readonly string UsersContainerID = "users";
        private static readonly string MessagesContainerID = "messages";
        private static readonly string ChatThreadsContainerID = "threads";

        private static CosmosClient cosmosClient = new CosmosClient(NetworkConstants.COSMOSDB_URI, PrimaryKey);
        private static Database database = cosmosClient.GetDatabase(DatabaseId);

        public static Container UsersContainer = database.GetContainer(UsersContainerID);
        public static Container MessagesContainer = database.GetContainer(MessagesContainerID);
        public static Container ChatThreadsContainer = database.GetContainer(ChatThreadsContainerID);
    }
}
