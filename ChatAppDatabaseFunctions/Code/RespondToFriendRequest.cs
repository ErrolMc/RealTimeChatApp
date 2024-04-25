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

namespace ChatAppDatabaseFunctions.Code
{
    public static class RespondToFriendRequest
    {
        [FunctionName("RespondToFriendRequest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RespondToFriendRequestData requestData = JsonConvert.DeserializeObject<RespondToFriendRequestData>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = "Invalid request data" });
            }

            try
            {
                User toUser = await SharedQueries.GetUserFromUserID(requestData.ToUserID);
                User fromUser = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
                if (toUser == null || fromUser == null)
                {
                    return new BadRequestObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} IsNull: {toUser == null} FromUser: {requestData.FromUserID} IsNull: {fromUser == null}" });
                }

                toUser.FriendRequests.Remove(fromUser.UserID);
                fromUser.OutgoingFriendRequests.Remove(toUser.UserID);

                if (requestData.Status)
                {
                    if (!toUser.Friends.Contains(fromUser.UserID))
                        toUser.Friends.Add(fromUser.UserID);

                    if (!fromUser.Friends.Contains(toUser.UserID))
                        fromUser.Friends.Add(toUser.UserID);
                }

                var toUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new BadRequestObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {requestData.FromUserID} Status: {fromUserReplaceResponse.StatusCode}" });
                }

                // send a notification to the user who sent the request
                NotificationData notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.FriendRequestRespond,
                    RecipientUserID = fromUser.UserID,
                    NotificationJson = JsonConvert.SerializeObject(new FriendRequestRespondNotification { Status = requestData.Status, ToUser = new UserSimple { UserID = toUser.UserID, UserName = toUser.Username } })
                };

                (bool, string) notificationResult = await SharedRequests.SendNotificationThroughSignalR(notificationData);

                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = true, Message = $"Successfully responded to friend request, Sent notification: {notificationResult.Item1}" });
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"Exception: {ex.Message} "});
            }
        }
    }
}
