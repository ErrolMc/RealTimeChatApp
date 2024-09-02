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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ChatAppDatabaseFunctions.Code
{
    public static class LoginFunc
    {
        private const int TOKEN_EXPIRY_TIME = 30;

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
                var token = GenerateJwtToken(loginData.UserName);
                return new OkObjectResult(new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = resp.user, LoginToken = token });
            }

            return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Wrong password!", User = null });
        }

        public static string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(DatabaseStatics.SECRET_LOGIN_KEY);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("username", username)
                }),
                Expires = DateTime.Now.AddDays(TOKEN_EXPIRY_TIME),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
