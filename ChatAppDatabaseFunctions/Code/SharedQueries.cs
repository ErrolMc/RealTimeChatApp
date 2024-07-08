using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared.Misc;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SharedQueries
    {
        private const string GENERIC_DATABASE_ERROR = "Database Connection/Query Error";

        public static async Task<(bool connectionSuccess, string message, User user)> GetUserFromUsername(string username)
        {
            try
            {
                IQueryable<User> query = DatabaseStatics.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.Username == username);
                FeedIterator<User> iterator = query.ToFeedIterator();
                FeedResponse<User> users = await iterator.ReadNextAsync();

                if (!users.Any())
                    return (true, $"Cant find user {username}", null);

                return (true, "Success", users.First());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserName Error {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, null);
            }
        }

        public static async Task<(bool connectionSuccess, string message, User user)> GetUserFromUserID(string userID)
        {
            try
            {
                var userResponse = await DatabaseStatics.UsersContainer.ReadItemAsync<User>(userID, new PartitionKey(userID));

                if (userResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"GetUserFromUserID Error {userResponse.StatusCode}");
                    return (true, $"Cant find user {userID}", null);
                }

                return (true, "Success", userResponse.Resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserID Error {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, null);
            }
        }

        public static async Task<(bool connectionSuccess, string message, List<User> users)> GetUsers(List<string> userIDs)
        {
            if (userIDs == null || userIDs.Count() == 0)
                return (false, "No user ids provided", new List<User>());

            try
            {
                string inClause = string.Join(", ", userIDs.Select(id => $"'{id}'"));
                string queryString = $"SELECT * FROM c WHERE c.id IN ({inClause})";
                QueryDefinition queryDefinition = new QueryDefinition(queryString);
                FeedIterator<User> queryResultSetIterator = DatabaseStatics.UsersContainer.GetItemQueryIterator<User>(queryDefinition);

                List<User> users = new List<User>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var user in currentResultSet)
                    {
                        users.Add(user);
                    }
                }

                if (userIDs.Count != users.Count)
                    return (false, "Coundn't get all users", users);

                return (true, "Successfully got users", users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUsers Error {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, new List<User>());
            }
        }

        public static async Task<(bool connectionSuccess, string message, List<Message> messages)> GetMessagesByThreadID(string threadID)
        {
            try
            {
                List<Message> messages = new List<Message>();
                IQueryable<Message> query = DatabaseStatics.MessagesContainer.GetItemLinqQueryable<Message>().Where(m => m.ThreadID == threadID);
                FeedIterator<Message> iterator = query.ToFeedIterator();

                while (iterator.HasMoreResults)
                {
                    FeedResponse<Message> response = await iterator.ReadNextAsync();
                    messages.AddRange(response.ToList());
                }
                return (true, $"Gotten {messages.Count} messages", messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving messages for thread ID {threadID}: {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, new List<Message>());
            }
        }

        public static async Task<(bool connectionSuccess, string message, GroupDM groupDM)> GetGroupDMFromGroupID(string groupID)
        {
            try
            {
                var groupResponse = await DatabaseStatics.GroupDMsContainer.ReadItemAsync<GroupDM>(groupID, new PartitionKey(groupID));

                if (groupResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"GetGroupDMFromGroupID Error {groupResponse.StatusCode}");
                    return (true, $"Cant find Group DM {groupID}", null);
                }

                return (true, "Success", groupResponse.Resource);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGroupDMFromGroupID Error {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, null);
            }
        }

        public static async Task<(bool connectionSuccess, string message, List<GroupDM> groupDMs)> GetGroupDMsFromIDs(List<string> groupIDs)
        {
            if (groupIDs == null || groupIDs.Count() == 0)
                return (false, "No group ids provided", new List<GroupDM>());

            try
            {
                string inClause = string.Join(", ", groupIDs.Select(id => $"'{id}'"));
                string queryString = $"SELECT * FROM c WHERE c.id IN ({inClause})";
                QueryDefinition queryDefinition = new QueryDefinition(queryString);
                FeedIterator<GroupDM> queryResultSetIterator = DatabaseStatics.GroupDMsContainer.GetItemQueryIterator<GroupDM>(queryDefinition);

                List<GroupDM> groupDMs = new List<GroupDM>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<GroupDM> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (GroupDM groupDM in currentResultSet)
                    {
                        groupDMs.Add(groupDM);
                    }
                }

                return (true, "Successfully got GroupDMs", groupDMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetGroupDMs Error {ex.Message}");
                return (false, GENERIC_DATABASE_ERROR, new List<GroupDM>());
            }
        }
    }
}
