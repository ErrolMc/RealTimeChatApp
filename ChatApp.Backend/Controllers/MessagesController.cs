using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Messages;
using Microsoft.Azure.Cosmos;
using ChatApp.Backend.Repositories;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesRepository _repository;
        private readonly NotificationService _notifications;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(MessagesRepository repository, NotificationService notifications, ILogger<MessagesController> logger)
        {
            _repository = repository;
            _notifications = notifications;
            _logger = logger;
        }

        [HttpPost("api/SendMessage")]
        public async Task<IActionResult> SendMessage()
        {
            _logger.LogInformation("SendMessage request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            SendMessageRequestData requestData = JsonConvert.DeserializeObject<SendMessageRequestData>(requestBody);

            if (requestData == null)
                return Ok(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = "Invalid message data" });

            try
            {
                var (result, recipientUserIds) = await _repository.SendMessage(requestData);

                if (result.Success && result.Message != null && recipientUserIds.Count > 0)
                {
                    string messageJson = JsonConvert.SerializeObject(result.Message);
                    NotificationType notificationType = (MessageType)requestData.MessageType == MessageType.DirectMessage
                        ? NotificationType.DirectMessage
                        : NotificationType.GroupDMMessage;

                    foreach (string userId in recipientUserIds)
                    {
                        await _notifications.SendNotificationThroughSignalR(new NotificationData()
                        {
                            NotificationType = (int)notificationType,
                            RecipientUserID = userId,
                            NotificationJson = messageJson
                        });
                    }
                }

                return Ok(result);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Conflict(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = "Message already exists!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = "Database error" });
            }
        }

        [HttpPost("api/GetMessages")]
        public async Task<IActionResult> GetMessages()
        {
            _logger.LogInformation("GetMessages request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            GetMessagesRequestData requestData = JsonConvert.DeserializeObject<GetMessagesRequestData>(requestBody);

            if (requestData == null)
                return Ok(new GetMessagesResponseData { Success = false, ResponseMessage = "Invalid request data" });

            var result = await _repository.GetMessages(requestData);
            return Ok(result);
        }
    }
}
