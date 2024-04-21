using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Notifications;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using User = ChatApp.Shared.Tables.User;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SendFriendRequest
    {
        [FunctionName("SendFriendRequest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FriendRequestNotification requestData = JsonConvert.DeserializeObject<FriendRequestNotification>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "Invalid request data" });
            }

            // get the users
            User toUser = await SharedQueries.GetUserFromUsername(requestData.ToUserName);
            if (toUser == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "No to user with that username!" });
            }

            User fromUser = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
            if (fromUser == null)
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "No from user with that userID!" });
            }

            // check friends already
            if (fromUser.Friends.Contains(toUser.UserID))
            {
                return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "The 2 users are already friends!" });
            }

            NotificationData notificationData = new NotificationData()
            {
                NotificationType = (int)NotificationType.FriendRequest,
                RecipientUserID = toUser.UserID,
                NotificationJson = JsonConvert.SerializeObject(requestData)
            };

            string notificationJSON = JsonConvert.SerializeObject(notificationData);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                string signalRServerEndpoint = $"{NetworkConstants.SIGNALR_URI}api/Notifications/send-notification";

                HttpContent content = new StringContent(notificationJSON, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(signalRServerEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult(new FriendRequestNotificationResponseData { Status = true, Message = "Sent friend request!" });
                }
                else
                {
                    return new BadRequestObjectResult(new FriendRequestNotificationResponseData { Status = false, Message = "Error connecting to SignalR server: " + response.ReasonPhrase });
                }
            }
        }


    }
}
