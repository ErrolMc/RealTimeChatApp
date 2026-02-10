using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Enums;
using ChatApp.Backend.Repositories;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly GroupsRepository _repository;
        private readonly NotificationService _notifications;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(GroupsRepository repository, NotificationService notifications, ILogger<GroupsController> logger)
        {
            _repository = repository;
            _notifications = notifications;
            _logger = logger;
        }

        [HttpPost("api/CreateGroupDM")]
        public async Task<IActionResult> CreateGroupDM()
        {
            _logger.LogInformation("CreateGroupDM request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            CreateGroupDMRequestData requestData = JsonConvert.DeserializeObject<CreateGroupDMRequestData>(requestBody);

            if (requestData == null)
                return Ok(new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Invalid request data" });

            var (result, notifyUserIds) = await _repository.CreateGroupDM(requestData);

            if (result.CreatedGroupSuccess && result.GroupDMSimple != null)
            {
                string groupJson = JsonConvert.SerializeObject(result.GroupDMSimple);
                foreach (string userId in notifyUserIds)
                {
                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.AddedToGroup,
                        RecipientUserID = userId,
                        NotificationJson = groupJson
                    });
                }
            }

            return Ok(result);
        }

        [HttpPost("api/GetGroupDMs")]
        public async Task<IActionResult> GetGroupDMs()
        {
            _logger.LogInformation("GetGroupDMs request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
                return Ok(new GetGroupDMsResponseData { Success = false, Message = "Invalid request data" });

            var result = await _repository.GetGroupDMs(requestData);
            return Ok(result);
        }

        [HttpPost("api/GetGroupParticipants")]
        public async Task<IActionResult> GetGroupParticipants()
        {
            _logger.LogInformation("GetGroupParticipants request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            string groupID = JsonConvert.DeserializeObject<string>(requestBody);

            if (string.IsNullOrEmpty(groupID))
                return Ok(new GetGroupParticipantsResponseData { Success = false, Message = "Invalid request data" });

            var result = await _repository.GetGroupParticipants(groupID);
            return Ok(result);
        }

        [HttpPost("api/AddFriendsToGroup")]
        public async Task<IActionResult> AddFriendsToGroup()
        {
            _logger.LogInformation("AddFriendsToGroup request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            AddFriendsToGroupDMRequestData requestData = JsonConvert.DeserializeObject<AddFriendsToGroupDMRequestData>(requestBody);

            if (requestData == null)
                return Ok(new AddFriendsToGroupDMResponseData { Success = false, Message = "Invalid request data" });

            var (result, addedUserIds, updatedUserIds) = await _repository.AddFriendsToGroup(requestData);

            if (result.Success && result.GroupDMSimple != null)
            {
                string groupJson = JsonConvert.SerializeObject(result.GroupDMSimple);

                foreach (string userId in addedUserIds)
                {
                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.AddedToGroup,
                        RecipientUserID = userId,
                        NotificationJson = groupJson
                    });
                }

                foreach (string userId in updatedUserIds)
                {
                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.GroupUpdated,
                        RecipientUserID = userId,
                        NotificationJson = groupJson
                    });
                }
            }

            return Ok(result);
        }

        [HttpPost("api/RemoveUserFromGroup")]
        public async Task<IActionResult> RemoveUserFromGroup()
        {
            _logger.LogInformation("RemoveUserFromGroup request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            RemoveFromGroupRequestData requestData = JsonConvert.DeserializeObject<RemoveFromGroupRequestData>(requestBody);

            if (requestData == null)
                return Ok(new RemoveFromGroupResponseData { Success = false, Message = "Invalid request data" });

            var (result, remainingParticipantIds, ownerUserID, groupDMSimple) = await _repository.RemoveUserFromGroup(requestData);

            if (result.Success && groupDMSimple != null)
            {
                string groupJson = JsonConvert.SerializeObject(groupDMSimple);

                if (requestData.Reason == GroupUpdateReason.UserKicked)
                {
                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.KickedFromGroup,
                        RecipientUserID = requestData.UserID,
                        NotificationJson = groupJson
                    });
                }

                foreach (string userId in remainingParticipantIds)
                {
                    if (requestData.Reason == GroupUpdateReason.UserKicked && userId == ownerUserID)
                        continue;

                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.GroupUpdated,
                        RecipientUserID = userId,
                        NotificationJson = groupJson
                    });
                }
            }

            return Ok(result);
        }

        [HttpPost("api/DeleteGroup")]
        public async Task<IActionResult> DeleteGroup()
        {
            _logger.LogInformation("DeleteGroup request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            string groupID = JsonConvert.DeserializeObject<string>(requestBody);

            if (string.IsNullOrEmpty(groupID))
                return Ok(new DeleteGroupDMResponseData { Success = false, Message = "Invalid request data" });

            var (result, notifyUserIds, groupDMSimple) = await _repository.DeleteGroup(groupID);

            if (groupDMSimple != null)
            {
                string groupJson = JsonConvert.SerializeObject(groupDMSimple);
                foreach (string userId in notifyUserIds)
                {
                    await _notifications.SendNotificationThroughSignalR(new NotificationData()
                    {
                        NotificationType = (int)NotificationType.GroupDeleted,
                        RecipientUserID = userId,
                        NotificationJson = groupJson
                    });
                }
            }

            return Ok(result);
        }
    }
}
