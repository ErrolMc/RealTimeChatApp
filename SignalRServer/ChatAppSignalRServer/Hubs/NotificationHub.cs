using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Diagnostics;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Notifications;

namespace ChatAppSignalRServer.Hubs
{
    public class NotificationHub : Hub
    {
        public static Dictionary<string, string> _connectedUsers = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            string? userId = Context.User?.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                System.Console.WriteLine("[NotificationHub] User Connected: " + userId);
                _connectedUsers[Context.ConnectionId] = userId;
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connectedUsers.ContainsKey(Context.ConnectionId))
            {
                System.Console.WriteLine("[NotificationHub] User Disconnected: " + _connectedUsers[Context.ConnectionId]);
                _connectedUsers.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async void SendDirectMessage(string toUserID, string messageJson)
        {
            NotificationData notificationData = new NotificationData()
            {
                RecipientUserID = toUserID,
                NotificationType = (int)NotificationType.DirectMessage,
                NotificationJson = messageJson,
            };

            await Clients.User(toUserID).SendAsync("OnNotificationReceived", JsonConvert.SerializeObject(notificationData));
        }

        public async void SendGroupDMMessage(string toUserID, string messageJson)
        {
            NotificationData notificationData = new NotificationData()
            {
                RecipientUserID = toUserID,
                NotificationType = (int)NotificationType.GroupDMMessage,
                NotificationJson = messageJson,
            };

            await Clients.User(toUserID).SendAsync("OnNotificationReceived", JsonConvert.SerializeObject(notificationData));
        }
    }
}
