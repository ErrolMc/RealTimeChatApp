using ChatApp.Shared.Notifications;
using Newtonsoft.Json;
using System.Text;

namespace ChatApp.Backend.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool, string)> SendNotificationThroughSignalR(NotificationData notification)
        {
            try
            {
                string notificationJSON = JsonConvert.SerializeObject(notification);
                HttpContent content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("/api/Notifications/send-notification", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Notification sent successfully");
                }
                else
                {
                    return (false, $"Error Sending Notification To SignalR Server: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"SendNotificationThroughSignalR Exception: {ex.Message}");
            }
        }
    }
}
