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

namespace ChatAppDatabaseFunctions.Code
{
    public static class SaveMessageToDB
    {
        [FunctionName(FunctionNames.SAVE_MESSAGE_TO_DB)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            SaveMessageRequestData requestData = JsonConvert.DeserializeObject<SaveMessageRequestData>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new SaveMessageResponseData { Success = false, ResponseMessage = "Invalid user data" });
            }

            User fromUser = await SharedQueries.GetUserFromUserID(requestData.FromUserID);
            if (fromUser == null)
            {
                return new BadRequestObjectResult(new SaveMessageResponseData { Success = false, ResponseMessage = $"Couldnt find user {requestData.FromUserID}" });
            }

            Message message = new Message()
            {
                ID = Guid.NewGuid().ToString(),
                FromUser = fromUser.ToUserSimple(),
                ThreadID = requestData.ThreadID,
                MessageContents = requestData.Message,
                MessageType = requestData.MessageType,
                TimeStamp = DateTime.UtcNow.Ticks,
            };

            try
            {
                ItemResponse<Message> resp = await DatabaseStatics.MessagesContainer.CreateItemAsync(message, new PartitionKey(message.ThreadID));
                return new OkObjectResult(new SaveMessageResponseData { Success = true, ResponseMessage = "Created message successfully", Message = message });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new ConflictObjectResult(new SaveMessageResponseData { Success = false, ResponseMessage = $"Message {message.ID} already exists!" });
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred: {ex.Message}");
                return new ObjectResult(new SaveMessageResponseData { Success = false, ResponseMessage = "Database error" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            };
        }
    }
}
