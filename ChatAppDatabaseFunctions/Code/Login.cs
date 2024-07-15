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
using ChatApp.Shared;
using User = ChatApp.Shared.Tables.User;

namespace ChatAppDatabaseFunctions.Code
{
    public static class Login
    {
        [FunctionName(FunctionNames.LOGIN)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            UserLoginData loginData = JsonConvert.DeserializeObject<UserLoginData>(requestBody);

            if (loginData == null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Invalid user data" });
            }

            var resp = await SharedQueries.GetUserFromUsername(loginData.UserName);

            if (resp.connectionSuccess == false)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = resp.message, User = null });
            }

            if (resp.user == null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Username doesnt exist!", User = null });
            }

            bool correctPassword = PasswordHasher.VerifyPassword(loginData.Password, resp.user.HashedPassword);

            if (correctPassword)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = resp.user });
            }

            return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Wrong password!", User = null });
        }
    }
}
