using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared.Constants;
using Microsoft.AspNetCore.Components.Routing;
using Newtonsoft.Json;
using System.Net.Http;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SharedRequests
    {
        public static async Task<(bool, string)> SendNotificationThroughSignalR(NotificationData notification)
        {
            string notificationJSON = JsonConvert.SerializeObject(notification);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                string signalRServerEndpoint = $"{NetworkConstants.SIGNALR_URI}api/Notifications/send-notification";

                HttpContent content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(signalRServerEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "Notification sent successfully");
                }
                else
                {
                    return (false, $"Error Sending Notification To SignalR Server: {response.ReasonPhrase}");
                }
            }
        }
    }
}
