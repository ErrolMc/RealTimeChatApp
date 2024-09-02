using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Shared.Authentication;
using ChatApp.Shared;
using System.Linq;

namespace ChatAppDatabaseFunctions.Code.Login
{
    public static class AutoLogin
    {
        [FunctionName(FunctionNames.AUTO_LOGIN)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AutoLoginData autoLoginData = JsonConvert.DeserializeObject<AutoLoginData>(requestBody);

            ClaimsPrincipal principal = ValidateToken(autoLoginData.Token);

            if (principal == null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Couldnt process login token", User = null });
            }

            Claim claim = principal.Claims.FirstOrDefault(c => c.Type == "username");

            if (claim == null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "Cant find Username in login token", User = null });
            }

            string userName = claim.Value;
            var userResp = await SharedQueries.GetUserFromUsername(userName);

            if (userResp.connectionSuccess == false)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = userResp.message, User = null });
            }

            if (userResp.user == null)
            {
                return new OkObjectResult(new UserLoginResponseData { Status = false, Message = "User doesnt exist!", User = null });
            }

            string newToken = LoginFunc.GenerateJwtToken(userName);
            return new OkObjectResult(new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = userResp.user, LoginToken = newToken });
        }

        private static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(DatabaseStatics.SECRET_LOGIN_KEY);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true, 
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
