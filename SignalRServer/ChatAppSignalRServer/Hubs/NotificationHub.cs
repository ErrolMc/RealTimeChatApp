using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChatAppSignalRServer.Hubs
{
    public class NotificationHub : Hub
    {
        private static readonly Dictionary<string, string> _connectedUsers = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            string? userId = Context.User?.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                Debug.WriteLine("User Connected: " + userId);
                _connectedUsers[Context.ConnectionId] = userId;
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connectedUsers.ContainsKey(Context.ConnectionId))
            {
                Debug.WriteLine("User Disconnected: " + _connectedUsers[Context.ConnectionId]);
                _connectedUsers.Remove(Context.ConnectionId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public async Task BroadcastMessage(string message)
        {
            await Clients.Others.SendAsync("OnMessageReceived", message);
        }
    }
}
