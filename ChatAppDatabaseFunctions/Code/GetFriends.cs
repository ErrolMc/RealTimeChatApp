using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Authentication;
using Microsoft.Azure.Cosmos;
using System.Linq;
using User = ChatApp.Shared.Tables.User;
using System.ComponentModel;
using ChatApp.Shared.Notifications;
using System.Collections.Generic;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetFriends
    {
        [FunctionName("GetFriends")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new GetFriendsResponseData { Success = false, Message = "Invalid request data" });
            }

            var userResponse = await DatabaseStatics.UsersContainer.ReadItemAsync<User>(requestData.UserID, new PartitionKey(requestData.UserID));
            if (userResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new BadRequestObjectResult(new GetFriendsResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.UserID} Status: {userResponse.StatusCode}" });
            }

            User user = userResponse.Resource;

            try
            {
                if (user.Friends == null || user.Friends.Count == 0)
                {
                    return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = "No friends found" });
                }

                string inClause = string.Join(", ", user.Friends.Select(id => $"'{id}'"));
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

                return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = $"{friends.Count} Friends retrieved", Friends = friends });
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"An error occurred: {ex.Message}");
                return new BadRequestObjectResult(new GetFriendsResponseData { Success = false, Message = "An error occurred while processing your request." });
            }
        }
    }
}
