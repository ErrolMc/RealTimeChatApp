using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using ChatApp.Shared.Authentication;
using ChatApp.Utils;

namespace ChatApp.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        public async UniTask<(bool, string)> TryLogin(string username, string password)
        {
            return await PerformRequest("login", username, password);
        }

        public async UniTask<(bool, string)> TryRegister(string username, string password)
        {
            return await PerformRequest("register", username, password);
        }

        private async UniTask<(bool, string)> PerformRequest(string endPoint, string username, string password)
        {
            UserLoginData loginData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            string json = JsonConvert.SerializeObject(loginData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + endPoint, jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return (false, "Register failed");
            }
            
            UserLoginResponseData responseData = JsonConvert.DeserializeObject<UserLoginResponseData>(request.downloadHandler.text);
            return (responseData.Status, responseData.Message);
        }
    }
}

