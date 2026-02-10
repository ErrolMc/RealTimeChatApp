using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ChatApp.Shared.Authentication;
using Microsoft.Azure.Cosmos;
using ChatApp.Backend.Repositories;

namespace ChatApp.Backend.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LoginRepository _repository;
        private readonly ILogger<LoginController> _logger;

        public LoginController(LoginRepository repository, ILogger<LoginController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("api/Login")]
        public async Task<IActionResult> Login()
        {
            _logger.LogInformation("Login request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            UserLoginData loginData = JsonConvert.DeserializeObject<UserLoginData>(requestBody);

            if (loginData == null)
                return Ok(new UserLoginResponseData { Status = false, Message = "Invalid user data" });

            var result = await _repository.Login(loginData);
            return Ok(result);
        }

        [HttpPost("api/Register")]
        public async Task<IActionResult> Register()
        {
            _logger.LogInformation("Register request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            UserLoginData loginData = JsonConvert.DeserializeObject<UserLoginData>(requestBody);

            if (loginData == null)
                return Ok(new UserLoginResponseData { Status = false, Message = "Invalid user data" });

            try
            {
                var result = await _repository.Register(loginData);
                return Ok(result);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return Conflict(new UserLoginResponseData { Status = false, Message = "Username already exists!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new UserLoginResponseData { Status = false, Message = "Database error" });
            }
        }

        [HttpPost("api/AutoLogin")]
        public async Task<IActionResult> AutoLogin()
        {
            _logger.LogInformation("AutoLogin request received.");
            string requestBody = await new StreamReader(Request.Body).ReadToEndAsync();
            AutoLoginData autoLoginData = JsonConvert.DeserializeObject<AutoLoginData>(requestBody);

            var result = await _repository.AutoLogin(autoLoginData);
            return Ok(result);
        }
    }
}
