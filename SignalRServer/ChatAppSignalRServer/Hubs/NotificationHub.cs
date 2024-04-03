using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalRServer.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task BroadcastMessage(string message)
        {
            await Clients.Others.SendAsync("OnMessageReceived", message);
        }
    }
}
