using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }
        
        public async Task<(bool success, string message, User user)> TryLogin(string username, string password)
        {
            var resp = await PerformRequest(FunctionNames.LOGIN, username, password);
            return (resp.Success, resp.Message, resp.ResponseData.User);
        }

        public async Task<(bool success, string message)> TryRegister(string username, string password)
        {
            var resp = await PerformRequest(FunctionNames.REGISTER, username, password);
            return (resp.Success, resp.Message);
        }
        
        private async Task<FunctionPostResponse<UserLoginResponseData>> PerformRequest(string functionName, string username, string password)
        {
            UserLoginData loginData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            var response = await NetworkHelper.PerformFunctionPostRequest<UserLoginData, UserLoginResponseData>(functionName, loginData);
            return response;
        }
    }
}