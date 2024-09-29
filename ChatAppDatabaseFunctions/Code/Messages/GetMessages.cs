using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.Misc;
using System.Collections.Generic;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Tables;
using ChatApp.Shared;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetMessages
    {
        [FunctionName(FunctionNames.GET_MESSAGES)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            GetMessagesRequestData requestData = JsonConvert.DeserializeObject<GetMessagesRequestData>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new GetMessagesResponseData { Success = false, ResponseMessage = "Invalid request data" });
            }

            var messagesResp = await SharedQueries.GetMessagesByThreadIDAfterTimeStamp(requestData.ThreadID, requestData.LocalTimeStamp);

            if (messagesResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetMessagesResponseData { Success = false, ResponseMessage = $"Failed to get messages for Thread {requestData.ThreadID}" });
            }

            return new OkObjectResult(new GetMessagesResponseData { Success = true, ResponseMessage = "Success", Messages = messagesResp.messages });
        }
    }
}
