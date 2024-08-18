using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using System.Threading;
using ChatApp.Shared.Notifications;

namespace ChatAppDatabaseFunctions.Code.Groups
{
    public static class DeleteGroup
    {
        [FunctionName("DeleteGroup")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string groupID = JsonConvert.DeserializeObject<string>(requestBody);

            if (string.IsNullOrEmpty(groupID))
            {
                return new OkObjectResult(new DeleteGroupDMResponseData { Success = false, Message = "Invalid request data" });
            }

            var groupDMResp = await SharedQueries.GetChatThreadFromThreadID(groupID);
            if (groupDMResp.connectionSuccess == false)
            {
                return new OkObjectResult(new DeleteGroupDMResponseData { Success = false, Message = $"Couldn't find group with id {groupID}" });
            }

            ChatThread groupDM = groupDMResp.thread;

            var getParticipantsResp = await SharedQueries.GetUsers(groupDM.Users);
            if (getParticipantsResp.connectionSuccess == false)
            {
                return new OkObjectResult(new DeleteGroupDMResponseData { Success = false, Message = "Couldn't get participant user info from database" });
            }

            // replace group
            try
            {
                var groupReplaceResponse = await DatabaseStatics.ChatThreadsContainer.DeleteItemAsync<ChatThread>(groupDM.ID, new PartitionKey(groupDM.ID));
                if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    return new OkObjectResult(new DeleteGroupDMResponseData { Success = false, Message = $"Couldn't delete group {groupID}" });
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new DeleteGroupDMResponseData { Success = false, Message = $"Group delete Exception: {ex.Message}" });
            }

            // delete messages
            var deleteMessagesResponse = await SharedQueries.DeleteMessagesByThreadID(groupDM.ID);
            if (deleteMessagesResponse.connectionSuccess == false)
            {
                Console.WriteLine($"Delete Group {groupDM.ID} could'nt delete messages");
            }

            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in getParticipantsResp.users)
            {
                user.GroupDMs.Remove(groupID);

                var replaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // TODO: if fail database update for at least one user, cancel the update for the rest of the users
                    Console.WriteLine($"DeleteGroup: Failed to update database for {user.UserID} with group {groupID}");
                    failedDatabaseUpdates.Add(user.UserID);
                    continue;
                }

                if (user.UserID == groupDM.OwnerUserID)
                    continue;

                NotificationData notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.GroupDeleted,
                    RecipientUserID = user.UserID,
                    NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                };

                (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
            }

            if (failedDatabaseUpdates.Count > 0)
                return new OkObjectResult(new DeleteGroupDMResponseData() { Success = false, ReplaceGroupSuccess = true, ReplaceUserSuccess = false, Message = $"Successfully deleted group! Coundn't update database for {failedDatabaseUpdates.Count}/{groupDM.Users.Count} users" });

            return new OkObjectResult(new DeleteGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = true, Message = $"Successfully deleted group!" });
        }
    }
}
