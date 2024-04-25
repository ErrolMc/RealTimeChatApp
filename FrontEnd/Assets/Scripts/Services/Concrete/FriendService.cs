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
        public List<UserSimple> FriendRequests { get; set; }
        public List<UserSimple> OutgoingFriendRequests { get; set; }
        public event Action<UserSimple> OnFriendRequestReceived;
        public event Action<UserSimple> OnFriendRequestCanceled;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;

        public void Initialize()
        {
            Friends = new List<UserSimple>();
            OutgoingFriendRequests = new List<UserSimple>();
            FriendRequests = new List<UserSimple>();
        }

        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification)
        {
            FriendRequests.Add(notification.FromUser);
            OnFriendRequestReceived?.Invoke(notification.FromUser);
        }
        
        /// <summary>
        /// When a friend request that this client sent gets responded to
        /// </summary>
        /// <param name="response">The response data</param>
        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response)
        {
            if (response.Status)
            {
                AddFriendToFriendsList(response.ToUser);
            }

            OutgoingFriendRequests.Remove(response.ToUser);
            OnFriendRequestRespondedTo?.Invoke(response);
        }

        public void AddFriendToFriendsList(UserSimple user)
        {
            if (!Friends.Contains(user))
                Friends.Add(user);
        }

        public void CancelFriendRequest(UserSimple user)
        {
            FriendRequests.Remove(user);
            OnFriendRequestCanceled?.Invoke(user);
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

        public async UniTask<bool> GetFriendRequests()
        {
            if (_authenticationService?.CurrentUser == null)
                return false;
            
            (bool success, string message, GetFriendRequestsResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<UserSimple, GetFriendRequestsResponseData>("getfriendrequests", _authenticationService.CurrentUser.ToUserSimple());
            
            if (success)
            {
                FriendRequests = responseData.FriendRequests;
                OutgoingFriendRequests = responseData.OutgoingFriendRequests;
                return true;
            }
            
            Debug.LogError($"GetFriendRequests Error: {message}");
            return false;
        }

        public async UniTask<(bool, string, UserSimple)> SendFriendRequest(string userName)
        {
            if (_authenticationService?.CurrentUser == null)
                return (false, "Current user is null", null);

            if (userName == _authenticationService.CurrentUser.Username)
                return (false, "Cant send a friend request to yourself!", null);
            
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
                return (false, "Request failed", null);
            }
            
            Debug.LogError("FriendService - AddFriend: " + responseData.Message);
            return (responseData.Status, responseData.Message, responseData.ToUser);
        }
        
        public async UniTask<(bool, string)> RespondToFriendRequest(string fromUserID, string toUserID, bool status, bool isCanceling = false)
        {
            var notificationData = new RespondToFriendRequestData()
            {   
                FromUserID = fromUserID,
                ToUserID = toUserID,
                Status = status,
                isCanceling = isCanceling
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
            if (_authenticationService?.CurrentUser == null)
                return new GetFriendsResponseData() { Success = false, Message = "Current User is null"};
            
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