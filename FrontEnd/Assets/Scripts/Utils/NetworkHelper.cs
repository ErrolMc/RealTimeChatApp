using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatApp.Shared.Constants;

namespace ChatApp.Utils
{
    public static class NetworkHelper
    {
        public static async UniTask<(bool, string, U)> PerformFunctionPostRequest<T, U>(string functionName, T requestData) 
            where T : class 
            where U : class
        {
            string json = JsonConvert.SerializeObject(requestData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/" + functionName, jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return (false, request.error, null);
            }
            
            U responseData = JsonConvert.DeserializeObject<U>(request.downloadHandler.text);
            return (true, "Request Success", responseData);
        }
    }
}