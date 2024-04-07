using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalRServer.Providers
{
    public class CustomUserIDProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Retrieve the UserID claim from the connected user
            var res = connection.User?.Claims.FirstOrDefault(c => c.Type == "userid")?.Value;
            if (res == null)
                return "";
            return res;
        }
    }
}
