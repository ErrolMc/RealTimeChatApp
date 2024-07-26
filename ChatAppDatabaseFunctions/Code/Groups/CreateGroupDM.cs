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
using ChatAppDatabaseFunctions.Code.Utils;

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
                return new OkObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Invalid request data" });
            }

            // get all the users
            var getParticipantsResp = await SharedQueries.GetUsers(requestData.Participants);
            if (getParticipantsResp.connectionSuccess == false)
            {
                return new OkObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get participant user info from database" });
            }

            List<User> participants = getParticipantsResp.users;
            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, requestData.Creator);

            if (!ownerFound)
            {
                return new OkObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get creator user info from database" });
            }

            // create the group data structure
            string threadID = Guid.NewGuid().ToString();
            GroupDM groupDM = new GroupDM()
            {
                ID = threadID,
                ThreadID = threadID,
                OwnerUserID = requestData.Creator,
                ParticipantUserIDs = requestData.Participants,
                HasCustomName = false,
                Name = participants.GetGroupName()
            };

            // create the group
            try
            {
                ItemResponse<GroupDM> createGroupResp = await DatabaseStatics.GroupDMsContainer.CreateItemAsync(groupDM, new PartitionKey(threadID));
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred when creating a group: {ex.Message}");
                return new OkObjectResult(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Database error" });
            };

            List<string> failedNotifications = new List<string>();
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in participants)
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
                    NotificationType = (int)NotificationType.AddedToGroup,
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
