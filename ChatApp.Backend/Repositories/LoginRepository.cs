using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Repositories
{
    public class LoginRepository
    {
        private const int TOKEN_EXPIRY_TIME = 30;

        private readonly DatabaseService _db;
        private readonly QueryService _queries;

        public LoginRepository(DatabaseService db, QueryService queries)
        {
            _db = db;
            _queries = queries;
        }

        public virtual async Task<UserLoginResponseData> Login(UserLoginData loginData)
        {
            var resp = await _queries.GetUserFromUsername(loginData.UserName);

            if (resp.IsSuccessful == false)
                return new UserLoginResponseData { Status = false, Message = resp.ErrorMessage, User = null };

            bool correctPassword = PasswordHasher.VerifyPassword(loginData.Password, resp.Data.HashedPassword);

            if (correctPassword)
            {
                var token = GenerateJwtToken(loginData.UserName);
                return new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = resp.Data, LoginToken = token };
            }

            return new UserLoginResponseData { Status = false, Message = "Wrong password!", User = null };
        }

        public async Task<UserLoginResponseData> Register(UserLoginData loginData)
        {
            var userResp = await _queries.GetUserFromUsername(loginData.UserName);

            if (userResp.IsException)
                return new UserLoginResponseData { Status = false, Message = userResp.ErrorMessage };

            if (userResp.Data != null)
                return new UserLoginResponseData { Status = false, Message = "Username already exists!" };

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

            ItemResponse<User> resp = await _db.UsersContainer.CreateItemAsync(newUser, new PartitionKey(userID));
            return new UserLoginResponseData { Status = true, Message = "Created account successfully!" };
        }

        public async Task<UserLoginResponseData> AutoLogin(AutoLoginData autoLoginData)
        {
            ClaimsPrincipal principal = ValidateToken(autoLoginData.Token);

            if (principal == null)
                return new UserLoginResponseData { Status = false, Message = "Couldnt process login token", User = null };

            Claim claim = principal.Claims.FirstOrDefault(c => c.Type == "username");

            if (claim == null)
                return new UserLoginResponseData { Status = false, Message = "Cant find Username in login token", User = null };

            string userName = claim.Value;
            var userResp = await _queries.GetUserFromUsername(userName);

            if (userResp.IsSuccessful == false)
                return new UserLoginResponseData { Status = false, Message = userResp.ErrorMessage, User = null };

            string newToken = GenerateJwtToken(userName);
            return new UserLoginResponseData { Status = true, Message = "Logged in successfully!", User = userResp.Data, LoginToken = newToken };
        }

        public static string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(DatabaseService.SECRET_LOGIN_KEY);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("username", username)
                }),
                Expires = DateTime.Now.AddDays(TOKEN_EXPIRY_TIME),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(DatabaseService.SECRET_LOGIN_KEY);
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
