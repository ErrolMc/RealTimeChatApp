using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Tables;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;
using System.Text.RegularExpressions;
using ChatApp.Shared.GroupDMs;
using System.Collections.Generic;
using System.Linq;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SendMessage
    {
        [FunctionName(FunctionNames.SEND_MESSAGE)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SendMessageRequestData requestData = JsonConvert.DeserializeObject<SendMessageRequestData>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = "Invalid message data" });
            }

            var fromUserResp = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
            if (fromUserResp.connectionSuccess == false)
            {
                return new OkObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = fromUserResp.message });
            }

            if (fromUserResp.user == null)
            {
                return new OkObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = $"Couldnt find user {requestData.FromUserID}" });
            }

            Message message = new Message()
            {
                ID = Guid.NewGuid().ToString(),
                FromUser = fromUserResp.user.ToUserSimple(),
                ThreadID = requestData.ThreadID,
                MessageContents = requestData.Message,
                MessageType = requestData.MessageType,
                TimeStamp = DateTime.UtcNow.Ticks,
            };

            ItemResponse<Message> messageResponse = null;

            try
            {
                messageResponse = await DatabaseStatics.MessagesContainer.CreateItemAsync(message, new PartitionKey(message.ThreadID));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new ConflictObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} already exists!" });
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred: {ex.Message}");
                return new ObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = "Database error" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            };

            if (messageResponse == null)
                return new OkObjectResult(new SendMessageResponseData { Success = false, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} couldnt be added to DB" });

            // Send notifications
            string messageJSON = JsonConvert.SerializeObject(message);

            switch ((MessageType)requestData.MessageType)
            {
                case MessageType.DirectMessage:
                    {
                        NotificationData notificationData = new NotificationData()
                        {
                            NotificationType = (int)NotificationType.DirectMessage,
                            RecipientUserID = requestData.MetaData,
                            NotificationJson = messageJSON
                        };

                        (bool result, string notificationResponseMessage) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
                    }
                    break;
                case MessageType.GroupMessage:
                    {
                        var getGroupDMResponse = await SharedQueries.GetGroupDMFromGroupID(requestData.ThreadID);
                        if (getGroupDMResponse.connectionSuccess == false)
                        {
                            return new OkObjectResult(new SendMessageResponseData { Success = true, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} added to DB but couldnt get group from DB to send notifications" });
                        }

                        var getParticipantsResp = await SharedQueries.GetUsers(getGroupDMResponse.groupDM.ParticipantUserIDs);
                        if (getParticipantsResp.connectionSuccess == false)
                        {
                            return new OkObjectResult(new SendMessageResponseData { Success = true, NotificationSuccess = false, ResponseMessage = $"Message {message.ID} added to DB but couldnt get group participants from DB to send notifications" });
                        }

                        List<User> participants = getParticipantsResp.users.Where(user => user.UserID != requestData.FromUserID).ToList();

                        foreach (User user in participants)
                        {
                            NotificationData notificationData = new NotificationData()
                            {
                                NotificationType = (int)NotificationType.GroupDMMessage,
                                RecipientUserID = user.UserID,
                                NotificationJson = messageJSON
                            };

                            (bool result, string notificationResponseMessage) = await SharedRequests.SendNotificationThroughSignalR(notificationData);
                        }
                    }
                    break;
            }

            return new OkObjectResult(new SendMessageResponseData { Success = true, NotificationSuccess = true, ResponseMessage = $"Message {message.ID} sent and added to DB" });
        }
    }
}
