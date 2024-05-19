using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Notifications;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatApp.Shared.Keys;
using ChatApp.Shared.Constants;
using System.Security.Cryptography.Xml;
using ChatApp.Shared;

namespace ChatAppDatabaseFunctions.Code
{
    public static class AuthenticateSignalRFunction
    {
        [FunctionName(FunctionNames.AUTHENTICATE_SIGNALR)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            AuthenticateRequestData requestData = JsonConvert.DeserializeObject<AuthenticateRequestData>(requestBody);

            if (requestData == null)
            {
                return new BadRequestObjectResult(new AuthenticateResponseData { Status = false, Message = "Bad Request Data", AccessToken = "" });
            }

            if (string.IsNullOrEmpty(requestData.UserName) || string.IsNullOrEmpty(requestData.UserID))
            {
                return new BadRequestObjectResult(new AuthenticateResponseData() { Status = false, Message = "Username or password not provided", AccessToken = null });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, requestData.UserName),
                new Claim("userid", requestData.UserID),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Keys.SIGNALR_AUTH_ISSUER_SIGNING_KEY));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: NetworkConstants.FUNCTIONS_URI,
                audience: NetworkConstants.SIGNALR_URI,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60), // token expiration time
                signingCredentials: creds);

            return new OkObjectResult(
                new AuthenticateResponseData() 
                { 
                    Status = true, 
                    Message = "Successfully created SignalR token", 
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token) 
                });
        }
    }
}
