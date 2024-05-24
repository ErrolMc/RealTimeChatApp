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
using User = ChatApp.Shared.Tables.User;
using System.Collections.Generic;
using ChatApp.Shared;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetFriendRequests
    {
        [FunctionName(FunctionNames.GET_FRIEND_REQUESTS)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new GetFriendRequestsResponseData { Success = false, Message = "Invalid request data" });
            }

            var userResp = await SharedQueries.GetUserFromUserID(requestData.UserID);

            if (userResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new GetFriendRequestsResponseData { Success = false, Message = userResp.message });
            }

            if (userResp.user == null)
            {
                return new BadRequestObjectResult(new GetFriendRequestsResponseData { Success = false, Message = $"Cant find user {requestData.UserID}" });
            }

            var reqResp = await SharedQueries.GetUsers(userResp.user.FriendRequests);
            var outResp = await SharedQueries.GetUsers(userResp.user.OutgoingFriendRequests);

            if (reqResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new GetFriendRequestsResponseData { Success = false, Message = reqResp.message });
            }

            if (outResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new GetFriendRequestsResponseData { Success = false, Message = outResp.message });
            }

            return new OkObjectResult(new GetFriendRequestsResponseData { Success = true, Message = "Success", FriendRequests = reqResp.users, OutgoingFriendRequests = outResp.users });
        }
    }
}
