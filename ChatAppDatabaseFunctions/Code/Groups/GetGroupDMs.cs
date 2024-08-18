using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared;
using ChatApp.Shared.TableDataSimple;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared.Misc;
using ChatApp.Shared.GroupDMs;
using System.Collections.Generic;
using System.Linq;
using ChatApp.Shared.ExtensionMethods;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetGroupDMs
    {
        [FunctionName(FunctionNames.GET_GROUP_DMS)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserSimple requestData = JsonConvert.DeserializeObject<UserSimple>(requestBody);

            if (requestData == null)
            {
                return new OkObjectResult(new GetGroupDMsResponseData { Success = false, Message = "Invalid request data" });
            }

            var userResp = await SharedQueries.GetUserFromUserID(requestData.UserID);
            if (userResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetGroupDMsResponseData { Success = false, Message = userResp.message });
            }

            var groupDMResp = await SharedQueries.GetChatThreadsFromIDs(userResp.user.GroupDMs);
            if (groupDMResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetGroupDMsResponseData { Success = false, Message = groupDMResp.message });
            }

            List<GroupDMSimple> groupDMSimples = groupDMResp.groupDMs.ToGroupDMSimpleList();

            return new OkObjectResult(new GetGroupDMsResponseData() { Success = true, Message = $"Gathered {groupDMSimples.Count} group dms", GroupDMs = groupDMSimples });
        }
    }
}
