using System;
using System.Collections.Generic;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace ChatApp.Services.Concrete
{
    public class FriendService : IFriendService, IInitializable
    {
        [Inject] private IAuthenticationService _authenticationService;

        public List<UserSimple> Friends { get; set; }
        public List<FriendRequestNotification> ReceivedFriendRequestsThisSession { get; set; }
        public event Action<FriendRequestNotification> OnFriendRequestReceived;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;

        public void Initialize()
        {
            Friends = new List<UserSimple>();
            ReceivedFriendRequestsThisSession = new List<FriendRequestNotification>();
        }

        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification)
        {
            ReceivedFriendRequestsThisSession.Add(notification);
            OnFriendRequestReceived?.Invoke(notification);
        }
        
        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response)
        {
            if (response.Status)
            {
                Friends.Add(response.ToUser);
            }
            
            OnFriendRequestRespondedTo?.Invoke(response);
        }

        public async UniTask<(bool, string)> AddFriend(string userName)
        {
            var notificationData = new FriendRequestNotification()
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
            Debug.LogError("FriendService - AddFriend: " + responseData.Message);
            return (responseData.Status, responseData.Message);
        }
        
        public async UniTask<(bool, string)> RespondToFriendRequest(string fromUserID, bool status)
        {
            var notificationData = new RespondToFriendRequestData()
            {   
                FromUserID = fromUserID,
                ToUserID = _authenticationService.CurrentUser.Username,
                Status = status
            };
            
            string json = JsonConvert.SerializeObject(notificationData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/respondtofriendrequest", jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("FriendService - Failed to respond to friend request");
                return (false, "Request failed");
            }
            
            RespondToFriendRequestResponseData responseData = JsonConvert.DeserializeObject<RespondToFriendRequestResponseData>(request.downloadHandler.text);
            Debug.LogError("FriendService - Respond to request: " + responseData.Message);
            return (responseData.Success, responseData.Message);
        }

        public async UniTask<bool> UpdateFriendsList()
        {
            Friends.Clear();
            
            GetFriendsResponseData response = await GetFriends();
            if (response.Success)
            {
                Friends = response.Friends;
            }

            return response.Success;
        }
        
        private async UniTask<GetFriendsResponseData> GetFriends()
        {
            UserSimple requestData = new UserSimple
            {   
                UserName = _authenticationService.CurrentUser.Username,
                UserID = _authenticationService.CurrentUser.UserID,
            };
            
            string json = JsonConvert.SerializeObject(requestData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/getfriends", jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("FriendService - Failed to respond to friend request");
                return new GetFriendsResponseData() { Success = false, Message = "Request failed" };
            }
            
            GetFriendsResponseData responseData = JsonConvert.DeserializeObject<GetFriendsResponseData>(request.downloadHandler.text);
            Debug.LogError("FriendService - GetFriends: " + responseData.Message);
            return responseData;
        }
    }
}