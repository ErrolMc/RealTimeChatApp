using ChatApp.Shared.Constants;
using ChatApp.Shared.Notifications;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace ChatApp.Services.Concrete
{
    public class FriendService : IFriendService
    {
        [Inject] private IAuthenticationService _authenticationService;
        
        public async UniTask<(bool, string)> AddFriend(string userName)
        {
            FriendRequestNotification notificationData = new FriendRequestNotification()
            {   
                FromUserID = _authenticationService.CurrentUser.UserID,
                FromUserName = _authenticationService.CurrentUser.Username,
                ToUserName = userName
            };
            
            string json = JsonConvert.SerializeObject(notificationData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/sendfriendrequest", jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("FriendService: Request failed");
                return (false, "Request failed");
            }
            
            FriendRequestNotificationResponseData responseData = JsonConvert.DeserializeObject<FriendRequestNotificationResponseData>(request.downloadHandler.text);
            Debug.LogError("FriendService: " + responseData.Message);
            return (responseData.Status, responseData.Message);
        }
    }
}