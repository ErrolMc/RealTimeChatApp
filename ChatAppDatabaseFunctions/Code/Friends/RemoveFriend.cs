using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Misc;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using System.Runtime.InteropServices;
using ChatApp.Shared;
using ChatApp.Shared.Tables;

namespace ChatAppDatabaseFunctions.Code
{
    public static class RemoveFriend
    {
        [FunctionName("RemoveFriend")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UnfriendNotification requestData = JsonConvert.DeserializeObject<UnfriendNotification>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = "Invalid request data" });
            }

            var toUserResp = await SharedQueries.GetUserFromUserID(requestData.ToUserID);
            if (toUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = toUserResp.message });
            }

            var fromUserResp = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
            if (toUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = fromUserResp.message });
            }

            User toUser = toUserResp.user;
            User fromUser = fromUserResp.user;

            if (toUser == null || fromUser == null)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} IsNull: {toUser == null} FromUser: {requestData.FromUserID} IsNull: {fromUser == null}" });
            }

            // remove thread
            try
            {
                string threadID = SharedStaticMethods.CreateHashedDirectMessageID(toUser.UserID, fromUser.UserID);
                var threadRemoveResponse = await DatabaseStatics.ChatThreadsContainer.DeleteItemAsync<ChatThread>(threadID, new PartitionKey(threadID));
                if (threadRemoveResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    return new OkObjectResult(new GenericResponseData { Success = false, Message = $"Couldnt remove thread from database" });
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = $"RemoveFriend Remove Thread Exception: {ex.Message}" });
            }

            toUser.Friends.Remove(fromUser.UserID);
            toUser.FriendsVNum++;

            fromUser.Friends.Remove(toUser.UserID);
            fromUser.FriendsVNum++;

            try
            {
                var toUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new OkObjectResult(new GenericResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {requestData.FromUserID} Status: {fromUserReplaceResponse.StatusCode}" });
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new GenericResponseData { Success = false, Message = $"RespondToFriendRequest Replace Exception: {ex.Message}" });
            }

            var deleteMessagesResponse = await SharedQueries.DeleteMessagesByThreadID(requestData.ThreadID);
            if (deleteMessagesResponse.connectionSuccess == false)
            {
                Console.WriteLine($"Remove friend from {requestData.FromUserID} to {requestData.ToUserID} could'nt delete messages");
            }

            NotificationData notificationData = new NotificationData()
            {
                NotificationType = (int)NotificationType.Unfriend,
                RecipientUserID = toUser.UserID,
                NotificationJson = JsonConvert.SerializeObject(requestData)
            };

            (bool, string) notificationResult = await SharedRequests.SendNotificationThroughSignalR(notificationData);

            return new OkObjectResult(new GenericResponseData { Success = true, Message = $"Successfully removed friend, Sent notification: {notificationResult.Item1}" });
        }
    }
}
