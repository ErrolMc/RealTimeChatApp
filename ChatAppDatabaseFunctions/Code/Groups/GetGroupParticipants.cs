using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatApp.Shared.ExtensionMethods;

namespace ChatAppDatabaseFunctions.Code
{
    public static class GetGroupParticipants
    {
        [FunctionName("GetGroupParticipants")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            string groupID = JsonConvert.DeserializeObject<string>(requestBody);

            if (string.IsNullOrEmpty(groupID))
            {
                return new OkObjectResult(new GetGroupParticipantsResponseData { Success = false, Message = "Invalid request data" });
            }

            var groupDMResp = await SharedQueries.GetGroupDMFromGroupID(groupID);
            if (groupDMResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetGroupParticipantsResponseData { Success = false, Message = groupDMResp.message });
            }

            GroupDM groupDM = groupDMResp.groupDM;

            var participantResp = await SharedQueries.GetUsers(groupDM.ParticipantUserIDs);
            if (participantResp.connectionSuccess == false)
            {
                return new OkObjectResult(new GetGroupParticipantsResponseData { Success = false, Message = participantResp.message });
            }

            return new OkObjectResult(new GetGroupParticipantsResponseData { Success = true, Message ="Success", OwnerUserID = groupDM.OwnerUserID, Participants = participantResp.users.ToUserSimpleList() });
        }
    }
}
