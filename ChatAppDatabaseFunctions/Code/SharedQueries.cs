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
        public static async Task<User> GetUserFromUsername(string username)
        {
            try
            {
                IQueryable<User> query = DatabaseStatics.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.Username == username);
                FeedIterator<User> iterator = query.ToFeedIterator();
                FeedResponse<User> users = await iterator.ReadNextAsync();

                if (!users.Any())
                    return null;

                return users.First();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserName Error {ex.Message}");
                return null;
            }
        }

        public static async Task<User> GetUserFromUserID(string userID)
        {
            try
            {
                var userResponse = await DatabaseStatics.UsersContainer.ReadItemAsync<User>(userID, new PartitionKey(userID));

                if (userResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"GetUserFromUserID Error {userResponse.StatusCode}");
                    return null;
                }

                return userResponse.Resource;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetUserFromUserID Error {ex.Message}");
                return null;
            }
        }

        public static async Task<(bool, string, List<UserSimple>)> GetUsers(List<string> userIDs)
        {
            if (userIDs == null || userIDs.Count() == 0)
                return (false, "No user ids provided", new List<UserSimple>());

            try
            {
                string inClause = string.Join(", ", userIDs.Select(id => $"'{id}'"));
                string queryString = $"SELECT * FROM c WHERE c.id IN ({inClause})";
                QueryDefinition queryDefinition = new QueryDefinition(queryString);
                FeedIterator<User> queryResultSetIterator = DatabaseStatics.UsersContainer.GetItemQueryIterator<User>(queryDefinition);

                List<UserSimple> friends = new List<UserSimple>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (var friend in currentResultSet)
                    {
                        friends.Add(new UserSimple(friend));
                    }
                }

                return (true, "Successfully got users", friends);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, new List<UserSimple>());
            }
        }
    }
}
