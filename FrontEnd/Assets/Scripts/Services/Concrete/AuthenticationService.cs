using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatApp.Shared.Constants;
using ChatApp.Utils;

namespace ChatApp.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }

        public async UniTask<(bool, string, User)> TryLogin(string username, string password)
        {
            return await PerformRequest("login", username, password);
        }

        public async UniTask<(bool, string)> TryRegister(string username, string password)
        {
            (bool, string, User) resp = await PerformRequest("register", username, password);
            return (resp.Item1, resp.Item2);
        }

        private async UniTask<(bool, string, User)> PerformRequest(string functionName, string username, string password)
        {
            UserLoginData loginData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            (bool success, string message, UserLoginResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<UserLoginData, UserLoginResponseData>(functionName, loginData);

            if (success == false)
                return (false, functionName + " failed", null);
            
            return (responseData.Status, responseData.Message, responseData.User);
        }
    }
}

