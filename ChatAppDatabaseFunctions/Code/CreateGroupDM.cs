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

namespace ChatAppDatabaseFunctions.Code
{
    public static class CreateGroupDM
    {
        [FunctionName(FunctionNames.CREATE_GROUP_DM)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CreateGroupDMRequestData requestData = JsonConvert.DeserializeObject<CreateGroupDMRequestData>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Invalid request data" });
            }

            // get all the users
            var getParticipantsResp = await SharedQueries.GetUsers(requestData.Participants);
            if (getParticipantsResp.connectionSuccess == false)
            {
                return new BadRequestObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get participant user info from database" });
            }

            var owner = getParticipantsResp.users.FirstOrDefault(user => user.UserID == requestData.Creator);
            if (owner == null)
            {
                return new BadRequestObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get creator user info from database" });
            }

            List<User> participantsWithOwnerAtFront = getParticipantsResp.users;
            if (participantsWithOwnerAtFront.Remove(owner))
                participantsWithOwnerAtFront.Insert(0, owner);

            // create the group data structure
            string threadID = Guid.NewGuid().ToString();
            GroupDM groupDM = new GroupDM()
            {
                ID = threadID,
                ThreadID = threadID,
                OwnerUserID = requestData.Creator,
                ParticipantUserIDs = requestData.Participants,
                HasCustomName = false,
                Name = string.Join(", ", participantsWithOwnerAtFront.Select(user => $"{user.Username}")).TrimEnd()
            };

            // create the group
            try
            {
                ItemResponse<GroupDM> createGroupResp = await DatabaseStatics.GroupDMsContainer.CreateItemAsync(groupDM, new PartitionKey(threadID));
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred when creating a group: {ex.Message}");
                return new BadRequestObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Database error" });
            };

            List<string> failedNotifications = new List<string>();
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in participantsWithOwnerAtFront)
            {
                // update each user in database with the group in their record
                if (!user.GroupDMs.Contains(threadID))
                    user.GroupDMs.Add(threadID);
                else
                    Console.WriteLine($"CreateGroupDM: For some reason user {user.UserID} is already in group {threadID}");

                var replaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    // TODO: if fail database update for at least one user, cancel the update for the rest of the users
                    Console.WriteLine($"CreateGroupDM: Failed to update database for {user.UserID} with group {threadID}");
                    failedDatabaseUpdates.Add(user.UserID);
                    continue;
                }

                // dont need to send notification to owner
                if (user.UserID == owner.UserID)
                    continue;

                // notify other users the group was created
                NotificationData notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.GroupCreated,
                    RecipientUserID = user.UserID,
                    NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                };

                (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);

                if (result == false)
                    failedNotifications.Add(user.UserID);
            }

            if (failedDatabaseUpdates.Count > 0)
                return new OkObjectResult(new CreateGroupDMResponseData() { CreatedGroupSuccess = true, UpdateDatabaseSuccess = false, Message = $"Successfully created group! Coundn't update database for {failedNotifications.Count}/{groupDM.ParticipantUserIDs.Count} users", GroupDMSimple = groupDM.ToGroupDMSimple() });
            
            return new OkObjectResult(new CreateGroupDMResponseData() { CreatedGroupSuccess = true, UpdateDatabaseSuccess = true, Message = $"Successfully created group!", GroupDMSimple = groupDM.ToGroupDMSimple() });
        }
    }
}
