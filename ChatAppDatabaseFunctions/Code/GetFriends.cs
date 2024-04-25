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

            User user = await SharedQueries.GetUserFromUserID(requestData.UserID);
            if (user == null)
            {
                return new BadRequestObjectResult(new GetFriendsResponseData { Success = false, Message = $"Cant find user {requestData.UserID}" });
            }

            if (user.Friends == null || user.Friends.Count == 0)
            {
                return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = "No friends found" });
            }

            (bool success, string message, List<UserSimple> friends) = await SharedQueries.GetUsers(user.Friends);

            if (success == false)
            {
                System.Console.WriteLine($"An error occurred: {message}");
                return new BadRequestObjectResult(new GetFriendsResponseData { Success = false, Message = "An error occurred while processing your request." });
            }

            return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = $"{friends.Count} Friends retrieved", Friends = friends });
        }
    }
}
