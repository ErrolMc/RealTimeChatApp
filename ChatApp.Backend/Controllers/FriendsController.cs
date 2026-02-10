using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Friends;
using ChatApp.Backend.Repositories;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    public class FriendsController : ControllerBase
    {
        private readonly FriendsRepository _repository;
        private readonly NotificationService _notifications;
        private readonly ILogger<FriendsController> _logger;

        public FriendsController(FriendsRepository repository, NotificationService notifications, ILogger<FriendsController> logger)
        {
            _repository = repository;
            _notifications = notifications;
            _logger = logger;
        }

        [HttpPost("api/SendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest()
        {
            _logger.LogInformation("SendFriendRequest request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            FriendRequestNotification requestData = JsonConvert.DeserializeObject<FriendRequestNotification>(requestBody);

            if (requestData == null)
                return Ok(new FriendRequestNotificationResponseData { Status = false, Message = "Invalid request data" });

            var result = await _repository.SendFriendRequest(requestData);

            if (result.Status)
            {
                var (success, message) = await _notifications.SendNotificationThroughSignalR(new NotificationData()
                {
                    NotificationType = (int)NotificationType.FriendRequest,
                    RecipientUserID = result.ToUser.UserID,
                    NotificationJson = JsonConvert.SerializeObject(requestData)
                });

                result.Message = message;
                if (!success) result.Status = false;
            }

            return Ok(result);
        }

        [HttpPost("api/RespondToFriendRequest")]
        public async Task<IActionResult> RespondToFriendRequest()
        {
            _logger.LogInformation("RespondToFriendRequest request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            RespondToFriendRequestData requestData = JsonConvert.DeserializeObject<RespondToFriendRequestData>(requestBody);

            if (requestData == null)
                return Ok(new RespondToFriendRequestResponseData { Success = false, Message = "Invalid request data" });

            var (result, fromUser, toUser) = await _repository.RespondToFriendRequest(requestData);

            if (result.Success)
            {
                NotificationData notificationData;
                if (requestData.isCanceling)
                {
                    notificationData = new NotificationData()
                    {
                        NotificationType = (int)NotificationType.FriendRequestCancel,
                        RecipientUserID = requestData.ToUserID,
                        NotificationJson = JsonConvert.SerializeObject(fromUser)
                    };
                }
                else
                {
                    notificationData = new NotificationData()
                    {
                        NotificationType = (int)NotificationType.FriendRequestRespond,
                        RecipientUserID = requestData.FromUserID,
                        NotificationJson = JsonConvert.SerializeObject(new FriendRequestRespondNotification { Status = requestData.Status, ToUser = toUser })
                    };
                }

                var (success, _) = await _notifications.SendNotificationThroughSignalR(notificationData);
                result.Message = $"{result.Message}, Sent notification: {success}";
            }

            return Ok(result);
        }

        [HttpPost("api/GetFriends")]
        public async Task<IActionResult> GetFriends()
        {
            _logger.LogInformation("GetFriends request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            GetFriendsRequestData requestData = JsonConvert.DeserializeObject<GetFriendsRequestData>(requestBody);

            if (requestData == null)
                return Ok(new GetFriendsResponseData { Success = false, HasUpdate = false, VNum = -1, Message = "Invalid request data" });

            var result = await _repository.GetFriends(requestData);
            return Ok(result);
        }

        [HttpPost("api/GetFriendRequests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            _logger.LogInformation("GetFriendRequests request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
                return Ok(new GetFriendRequestsResponseData { Success = false, Message = "Invalid request data" });

            var result = await _repository.GetFriendRequests(requestData);
            return Ok(result);
        }

        [HttpPost("api/RemoveFriend")]
        public async Task<IActionResult> RemoveFriend()
        {
            _logger.LogInformation("RemoveFriend request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            UnfriendNotification requestData = JsonConvert.DeserializeObject<UnfriendNotification>(requestBody);

            if (requestData == null)
                return Ok(new GenericResponseData { Success = false, Message = "Invalid request data" });

            var result = await _repository.RemoveFriend(requestData);

            if (result.Success)
            {
                var (success, _) = await _notifications.SendNotificationThroughSignalR(new NotificationData()
                {
                    NotificationType = (int)NotificationType.Unfriend,
                    RecipientUserID = requestData.ToUserID,
                    NotificationJson = JsonConvert.SerializeObject(requestData)
                });

                result.Message = $"{result.Message}, Sent notification: {success}";
            }

            return Ok(result);
        }
    }
}
