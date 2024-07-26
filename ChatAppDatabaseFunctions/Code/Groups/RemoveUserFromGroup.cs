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
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using User = ChatApp.Shared.Tables.User;
using Microsoft.Azure.Cosmos;
using ChatApp.Shared;
using System.Collections.Generic;
using System.Linq;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Enums;
using ChatAppDatabaseFunctions.Code.Utils;

namespace ChatAppDatabaseFunctions.Code.Groups
{
    public static class RemoveUserFromGroup
    {
        [FunctionName(FunctionNames.REMOVE_USER_FROM_GROUP)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            RemoveFromGroupRequestData requestData = JsonConvert.DeserializeObject<RemoveFromGroupRequestData>(requestBody);

            // get all the data
            if (requestData == null)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = "Invalid request data" });
            }

            var groupDMResp = await SharedQueries.GetGroupDMFromGroupID(requestData.GroupID);
            if (groupDMResp.connectionSuccess == false)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't find group with id {requestData.GroupID}" });
            }

            var getParticipantsResp = await SharedQueries.GetUsers(groupDMResp.groupDM.ParticipantUserIDs);
            if (getParticipantsResp.connectionSuccess == false)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = "Couldn't get participant user info from database" });
            }

            // update the data
            GroupDM groupDM = groupDMResp.groupDM;
            List<User> participants = getParticipantsResp.users;
            User userToRemove = participants.FirstOrDefault(u => u.UserID == requestData.UserID);

            if (userToRemove == null)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't find user {requestData.UserID} in group {requestData.GroupID}" });
            }

            bool updatedGroup = groupDM.ParticipantUserIDs.Remove(requestData.UserID);
            bool updatedUser = userToRemove.GroupDMs.Remove(requestData.GroupID);

            if (!updatedGroup && !updatedUser)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't remove user {userToRemove.UserID} from group {groupDM.ID}, UpdateUser: {updatedUser}, UpdateGroup: {updatedGroup}" });
            }

            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, groupDM.OwnerUserID);
            if (!ownerFound)
            {
                return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = "Couldn't get creator user info from database" });
            }

            if (participants.Remove(userToRemove))
            {
                updatedGroup = true;
                groupDM.Name = participants.GetGroupName();
            }

            // replace group
            if (updatedGroup)
            {
                try
                {
                    var groupReplaceResponse = await DatabaseStatics.GroupDMsContainer.ReplaceItemAsync(groupDM, groupDM.ID, new PartitionKey(groupDM.ThreadID));
                    if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't replace group {requestData.GroupID} after removing user {requestData.UserID}" });
                    }
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" });
                }
            }

            // replace user
            if (updatedUser)
            {
                try
                {
                    var userReplaceResponse = await DatabaseStatics.UsersContainer.ReplaceItemAsync(userToRemove, userToRemove.UserID, new PartitionKey(userToRemove.UserID));
                    if (userReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't replace user {requestData.UserID} after removing group {requestData.GroupID}" });
                    }
                }
                catch (Exception ex)
                {
                    return new OkObjectResult(new RemoveFromGroupResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" });
                }
            }

            // send notification to kicked user
            if (requestData.Reason == GroupUpdateReason.UserKicked)
            {
                NotificationData notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.KickedFromGroup,
                    RecipientUserID = userToRemove.UserID,
                    NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                };

                (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
            }

            // send notifications to other group users
            foreach (User user in participants)
            {
                // dont send notification to the owner if the user was kicked
                if (requestData.Reason == GroupUpdateReason.UserKicked && user.UserID == groupDM.OwnerUserID)
                    continue;

                NotificationData notificationData = new NotificationData()
                {
                    NotificationType = (int)NotificationType.GroupUpdated,
                    RecipientUserID = user.UserID,
                    NotificationJson = JsonConvert.SerializeObject(groupDM.ToGroupDMSimple())
                };

                (bool result, string message) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
            }

            return new OkObjectResult(new RemoveFromGroupResponseData() { Success = true, Message = "Successfully removed user from group!", GroupName = groupDM.Name });
        }
    }
}
