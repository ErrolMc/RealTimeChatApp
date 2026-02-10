using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using ChatApp.Shared.Notifications;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatApp.Shared.Keys;
using ChatApp.Shared.Constants;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    public class AuthenticateSignalRController : ControllerBase
    {
        private readonly ILogger<AuthenticateSignalRController> _logger;

        public AuthenticateSignalRController(ILogger<AuthenticateSignalRController> logger)
        {
            _logger = logger;
        }

        [HttpPost("api/AuthenticateSignalR")]
        public async Task<IActionResult> Run()
        {
            _logger.LogInformation("AuthenticateSignalR request received.");

            string requestBody = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
            AuthenticateRequestData requestData = JsonConvert.DeserializeObject<AuthenticateRequestData>(requestBody);

            if (requestData == null)
                return Ok(new AuthenticateResponseData { Status = false, Message = "Bad Request Data", AccessToken = "" });

            if (string.IsNullOrEmpty(requestData.UserName) || string.IsNullOrEmpty(requestData.UserID))
                return Ok(new AuthenticateResponseData() { Status = false, Message = "Username or password not provided", AccessToken = null });

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
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds);

            var result = new AuthenticateResponseData()
            {
                Status = true,
                Message = "Successfully created SignalR token",
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token)
            };

            return Ok(result);
        }
    }
}
