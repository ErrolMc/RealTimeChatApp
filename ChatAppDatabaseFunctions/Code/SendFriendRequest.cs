using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Notifications;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared.Misc;
using ChatApp.Shared;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SendFriendRequest
    {
        [FunctionName(FunctionNames.SEND_FRIEND_REQUEST)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FriendRequestNotification requestData = JsonConvert.DeserializeObject<FriendRequestNotification>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "Invalid request data" });
            }

            // get the users
            var toUserResp = await SharedQueries.GetUserFromUsername(requestData.ToUserName);
            if (toUserResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = toUserResp.message });
            }

            if (toUserResp.user == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "No to user with that username!" });
            }

            var fromUserResp = await SharedQueries.GetUserFromUserID(requestData.FromUser.UserID);
            if (fromUserResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = fromUserResp.message });
            }

            if (fromUserResp.user == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "No from user with that username!" });
            }

            User toUser = toUserResp.user;
            User fromUser = fromUserResp.user;

            // check friends already
            if (fromUser.Friends.Contains(toUser.UserID))
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "The 2 users are already friends!" });
            }

            // check sent request already
            if (toUser.FriendRequests.Contains(fromUser.UserID) || fromUser.OutgoingFriendRequests.Contains(toUser.UserID))
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "Friend request already sent to this user!" });
            }

            toUser.FriendRequests.Add(fromUser.UserID);
            fromUser.OutgoingFriendRequests.Add(toUser.UserID);

            try
            {
                var toUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = $"Couldnt get users from database - ToUser: {toUser.UserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {fromUser.UserID} Status: {fromUserReplaceResponse.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = $"SendFriendRequest Replace Exception: {ex.Message}"});
            }

            NotificationData notificationData = new NotificationData()
            {
                NotificationType = (int)NotificationType.FriendRequest,
                RecipientUserID = toUser.UserID,
                NotificationJson = JsonConvert.SerializeObject(requestData)
            };

            (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);

            if (result == true)
            {
                return new OkObjectResult(new FriendRequestNotificationResponseData { Status = true, Message = message, ToUser = toUser.ToUserSimple() });
            }

            return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = message });
        }
    }
}
