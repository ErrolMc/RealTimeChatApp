using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatApp.Shared.Constants;

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

        private async UniTask<(bool, string, User)> PerformRequest(string endPoint, string username, string password)
        {
            UserLoginData loginData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            string json = JsonConvert.SerializeObject(loginData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/" + endPoint, jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return (false, "Register failed", null);
            }
            
            UserLoginResponseData responseData = JsonConvert.DeserializeObject<UserLoginResponseData>(request.downloadHandler.text);
            return (responseData.Status, responseData.Message, responseData.User);
        }
    }
}

