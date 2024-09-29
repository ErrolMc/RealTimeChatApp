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
using ChatApp.Shared;
using ChatApp.Shared.Tables;
using System.Collections.Generic;
using ChatApp.Shared.GroupDMs;
using System.Text.RegularExpressions;
using System.Threading;
using ChatApp.Shared.Friends;

namespace ChatAppDatabaseFunctions.Code
{
    public static class RespondToFriendRequest
    {
        [FunctionName(FunctionNames.RESPOND_TO_FRIEND_REQUEST)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RespondToFriendRequestData requestData = JsonConvert.DeserializeObject<RespondToFriendRequestData>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = "Invalid request data" });
            }

            var toUserResp = await SharedQueries.GetUserFromUserID(requestData.ToUserID);
            if (toUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = toUserResp.message });
            }

            var fromUserResp = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
            if (fromUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = fromUserResp.message });
            }

            User toUser = toUserResp.user;
            User fromUser = fromUserResp.user;

            if (toUser == null || fromUser == null)
            {
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} IsNull: {toUser == null} FromUser: {requestData.FromUserID} IsNull: {fromUser == null}" });
            }

            // create thread
            ChatThread thread = new ChatThread()
            {
                ID = SharedStaticMethods.CreateHashedDirectMessageID(fromUser.UserID, toUser.UserID),
                IsGroupDM = false,
                Users = new List<string> { fromUser.UserID, toUser.UserID },
                OwnerUserID = null,
                Name = null,
            };

            try
            {
                ItemResponse<ChatThread> createThreadResp = await DatabaseStatics.ChatThreadsContainer.CreateItemAsync(thread, new PartitionKey(thread.ID));
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred when creating thread for friend DM: {ex.Message}");
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = "Error when creating thread" });
            };

            toUser.FriendRequests.Remove(fromUser.UserID);
            fromUser.OutgoingFriendRequests.Remove(toUser.UserID);

            if (requestData.Status)
            {
                if (!toUser.Friends.Contains(fromUser.UserID))
                {
                    toUser.Friends.Add(fromUser.UserID);
                    toUser.FriendsVNum++;
                }

                if (!fromUser.Friends.Contains(toUser.UserID))
                {
                    fromUser.Friends.Add(toUser.UserID);
                    fromUser.FriendsVNum++;
                }
            }

            try
            {
                var toUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {requestData.FromUserID} Status: {fromUserReplaceResponse.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new RespondToFriendRequestResponseData { Success = false, Message = $"RespondToFriendRequest Replace Exception: {ex.Message}" });
            }

            NotificationData notificationData = new NotificationData();
            if (requestData.isCanceling)
            {
                notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.FriendRequestCancel,
                    RecipientUserID = toUser.UserID,
                    NotificationJson = JsonConvert.SerializeObject(fromUser.ToUserSimple())
                };
            }
            else
            {
                notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.FriendRequestRespond,
                    RecipientUserID = fromUser.UserID,
                    NotificationJson = JsonConvert.SerializeObject(new FriendRequestRespondNotification { Status = requestData.Status, ToUser = toUser.ToUserSimple() })
                };
            }

            (bool, string) notificationResult = await SharedRequests.SendNotificationThroughSignalR(notificationData);

            return new OkObjectResult(new RespondToFriendRequestResponseData { Success = true, Message = $"Successfully responded to friend request, Sent notification: {notificationResult.Item1}" });
        }
    }
}
