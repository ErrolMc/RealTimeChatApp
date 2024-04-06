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

namespace ChatAppDatabaseFunctions.Code
{
    public static class Login
    {
        [FunctionName("Login")]
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

            IQueryable<User> query = DatabaseStatics.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.Username == loginData.UserName);
            FeedIterator<User> iterator = query.ToFeedIterator();
            FeedResponse<User> users = await iterator.ReadNextAsync();

            if (!users.Any())
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "No user with that username!", User = null });
            }

            User user = users.First();
            bool correctPassword = PasswordHasher.VerifyPassword(loginData.Password, user.HashedPassword);

            if (correctPassword)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = user });
            }

            return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Wrong password!", User = null });
        }
    }
}
