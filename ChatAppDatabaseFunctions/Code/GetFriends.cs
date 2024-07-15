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
using ChatApp.Shared;
using ChatApp.Shared.ExtensionMethods;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetFriends
    {
        [FunctionName(FunctionNames.GET_FRIENDS)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new GetFriendsResponseData { Success = false, Message = "Invalid request data" });
            }

            var userResp = await SharedQueries.GetUserFromUserID(requestData.UserID);
            if (userResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetFriendsResponseData { Success = false, Message = userResp.message });
            }

            if (userResp.user.Friends == null || userResp.user.Friends.Count == 0)
            {
                return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = "No friends found" });
            }

            (bool success, string message, List<User> friends) = await SharedQueries.GetUsers(userResp.user.Friends);

            if (success == false)
            {
                System.Console.WriteLine($"An error occurred: {message}");
                return new OkObjectResult(new GetFriendsResponseData { Success = false, Message = "An error occurred while getting friends" });
            }

            return new OkObjectResult(new GetFriendsResponseData { Success = true, Message = $"{friends.Count} Friends retrieved", Friends = friends.ToUserSimpleList() });
        }
    }
}
