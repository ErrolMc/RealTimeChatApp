using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChatAppSignalRServer.Hubs
{
    public class NotificationHub : Hub
    {
        public static Dictionary<string, string> _connectedUsers = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            System.Console.WriteLine("[NotificationHub] OnConnectedAsync");

            string? userId = Context.User?.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                System.Console.WriteLine("[NotificationHub] User Connected: " + userId);
                _connectedUsers[Context.ConnectionId] = userId;
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            System.Console.WriteLine("[NotificationHub] OnDisconnectedAsync");
            if (_connectedUsers.ContainsKey(Context.ConnectionId))
            {
                System.Console.WriteLine("[NotificationHub] User Disconnected: " + _connectedUsers[Context.ConnectionId]);
                _connectedUsers.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
