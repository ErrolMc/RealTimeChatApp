using System;
using System.Collections.Generic;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using ChatApp.Utils;
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
                AddFriendToFriendsList(response.ToUser);
            }
            
            OnFriendRequestRespondedTo?.Invoke(response);
        }

        public void AddFriendToFriendsList(UserSimple user)
        {
            Friends.Add(user);
        }

        public void RespondToFriendRequest(FriendRequestNotification notification, bool status)
        {
            ReceivedFriendRequestsThisSession.Remove(notification);
            if (status)
                AddFriendToFriendsList(notification.FromUser);
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

        public async UniTask<(bool, string)> AddFriend(string userName)
        {
            var notificationData = new FriendRequestNotification()
            {   
                FromUser = _authenticationService.CurrentUser.ToUserSimple(),
                ToUserName = userName
            };
            
            (bool success, string message, FriendRequestNotificationResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<FriendRequestNotification, FriendRequestNotificationResponseData>("sendfriendrequest", notificationData);

            if (success == false)
            {
                Debug.LogError("FriendService: Request failed");
                return (false, "Request failed");
            }
            
            Debug.LogError("FriendService - AddFriend: " + responseData.Message);
            return (responseData.Status, responseData.Message);
        }
        
        public async UniTask<(bool, string)> RespondToFriendRequest(string fromUserID, bool status)
        {
            var notificationData = new RespondToFriendRequestData()
            {   
                FromUserID = fromUserID,
                ToUserID = _authenticationService.CurrentUser.UserID,
                Status = status
            };
            
            (bool success, string message, RespondToFriendRequestResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<RespondToFriendRequestData, RespondToFriendRequestResponseData>("respondtofriendrequest", notificationData);

            if (success == false)
            {
                Debug.LogError("FriendService - Failed to respond to friend request");
                return (false, "Request failed");
            }
            
            Debug.LogError("FriendService - Respond to request: " + responseData.Message);
            return (responseData.Success, responseData.Message);
        }
        
        private async UniTask<GetFriendsResponseData> GetFriends()
        {
            UserSimple requestData = new UserSimple
            {   
                UserName = _authenticationService.CurrentUser.Username,
                UserID = _authenticationService.CurrentUser.UserID,
            };
            
            (bool success, string message, GetFriendsResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<UserSimple, GetFriendsResponseData>("getfriends", requestData);

            if (success == false)
            {
                Debug.LogError("FriendService - Failed to respond to friend request");
                return new GetFriendsResponseData() { Success = false, Message = "Request failed" };
            }
            
            Debug.LogError("FriendService - GetFriends: " + responseData.Message);
            return responseData;
        }
    }
}