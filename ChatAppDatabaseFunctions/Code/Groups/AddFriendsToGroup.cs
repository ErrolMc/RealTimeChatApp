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
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Authentication;
using Microsoft.Azure.Cosmos;
using ChatApp.Shared.Tables;
using User = ChatApp.Shared.Tables.User;
using System.Collections.Generic;
using ChatApp.Shared;
using System.Linq;
using Microsoft.AspNetCore.Components.Routing;
using System.Threading;
using ChatAppDatabaseFunctions.Code.Utils;

namespace ChatAppDatabaseFunctions.Code.Groups
{
    public static class AddFriendsToGroup
    {
        [FunctionName(FunctionNames.ADD_FRIENDS_TO_GROUP)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AddFriendsToGroupDMRequestData requestData = JsonConvert.DeserializeObject<AddFriendsToGroupDMRequestData>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = "Invalid request data" });
            }

            var groupDMResp = await SharedQueries.GetChatThreadFromThreadID(requestData.GroupID);
            if (groupDMResp.connectionSuccess == false)
            {
                return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = $"Couldn't find group with id {requestData.GroupID}" });
            }

            // add user ids to group
            ChatThread groupDM = groupDMResp.thread;
            HashSet<string> usersToAdd = requestData.UsersToAdd.ToHashSet();

            foreach (string userID in requestData.UsersToAdd)
            {
                if (groupDM.Users.Contains(userID))
                    usersToAdd.Remove(userID);
                else
                    groupDM.Users.Add(userID);
            }

            var getUserResp = await SharedQueries.GetUsers(groupDM.Users);
            if (getUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = "Couldn't get participant user info from database" });
            }

            List<User> participants = getUserResp.users;
            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, groupDM.OwnerUserID);

            if (!ownerFound)
            {
                return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = "Couldn't get creator user info from database" });
            }

            groupDM.Name = participants.GetGroupName();

            // replace group
            try
            {
                var groupReplaceResponse = await DatabaseStatics.ChatThreadsContainer.ReplaceItemAsync(groupDM, groupDM.ID, new PartitionKey(groupDM.ID));
                if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = $"Couldn't replace group {requestData.GroupID}" });
                }
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new AddFriendsToGroupDMResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" });
            }

            // replace new users and send notifications
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in participants)
            {
                if (user.UserID == groupDM.OwnerUserID)
                    continue;

                bool isNewUser = usersToAdd.Contains(user.ID);
                if (isNewUser)
                {
                    if (!user.GroupDMs.Contains(groupDM.ID))
                        user.GroupDMs.Add(groupDM.ID);
                    else
                        Console.WriteLine($"AddFriendsToGroup: For some reason user {user.UserID} is already in group {groupDM.ID}");

                    var replaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                    if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        // TODO: if fail database update for at least one user, cancel the update for the rest of the users
                        Console.WriteLine($"AddFriendsToGroup: Failed to update database for {user.UserID} with group {groupDM.ID}");
                        failedDatabaseUpdates.Add(user.UserID);
                        continue;
                    }

                    NotificationData notificationData = new NotificationData()
                    {
                        NotificationType = (int)NotificationType.AddedToGroup,
                        RecipientUserID = user.UserID,
                        NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                    };

                    (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
                }
                else
                {
                    NotificationData notificationData = new NotificationData()
                    {
                        NotificationType = (int)NotificationType.GroupUpdated,
                        RecipientUserID = user.UserID,
                        NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                    };

                    (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
                }
            }

            if (failedDatabaseUpdates.Count > 0)
                return new OkObjectResult(new AddFriendsToGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = false, Message = $"Successfully updated group after adding users! Coundn't update database for {failedDatabaseUpdates.Count}/{usersToAdd.Count} users", GroupDMSimple = groupDM.ToGroupDMSimple() });

            return new OkObjectResult(new AddFriendsToGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = true, Message = $"Successfully added users to group!", GroupDMSimple = groupDM.ToGroupDMSimple() });
        }
    }
}
