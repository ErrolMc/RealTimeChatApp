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
using ChatApp.Shared.Tables;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;

namespace ChatAppDatabaseFunctions.Code
{
    public static class Register
    {
        [FunctionName(FunctionNames.REGISTER)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserLoginData loginData = JsonConvert.DeserializeObject<UserLoginData>(requestBody);

            if (loginData == null)
            {
                return new BadRequestObjectResult(new UserLoginResponseData { Status = false, Message = "Invalid user data" });
            }

            var userResp = await SharedQueries.GetUserFromUsername(loginData.UserName);

            if (userResp.connectionSuccess == false)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = userResp.message });
            }

            if (userResp.user != null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Username already exists!" });
            }

            string userID = Guid.NewGuid().ToString();
            User newUser = new User()
            {
                ID = userID,
                UserID = userID,
                Username = loginData.UserName,
                HashedPassword = PasswordHasher.HashPassword(loginData.Password),
                IsOnline = false,
                RealTimeChatConnectionID = "",
            };

            try
            {
                ItemResponse<User> resp = await DatabaseStatics.UsersContainer.CreateItemAsync(newUser, new PartitionKey(userID));
                return new OkObjectResult(new UserLoginResponseData { Status = true, Message = "Created account successfully!" });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new ConflictObjectResult(new UserLoginResponseData { Status = false, Message = "Username already exists!" });
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred: {ex.Message}");
                return new ObjectResult(new UserLoginResponseData { Status = false, Message = "Database error" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            };
        }
    }
}
