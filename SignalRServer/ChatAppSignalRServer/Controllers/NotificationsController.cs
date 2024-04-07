using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ChatAppSignalRServer.Hubs;
using ChatApp.Shared.Notifications;
using Newtonsoft.Json;

namespace ChatAppSignalRServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send-notification")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationData data)
        {
            System.Console.WriteLine("NotificationsController: SendNotification");

            if (data == null || string.IsNullOrWhiteSpace(data.RecipientUserID) || string.IsNullOrWhiteSpace(data.NotificationJson))
            {
                return BadRequest("Invalid notification data.");
            }

            // Send the notification to the specified user
            await _hubContext.Clients.User(data.RecipientUserID).SendAsync("OnNotificationReceived", JsonConvert.SerializeObject(data));
            return Ok();
        }
    }
}
