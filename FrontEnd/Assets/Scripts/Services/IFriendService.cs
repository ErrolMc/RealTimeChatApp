using System;
using ChatApp.Shared.Notifications;
using System.Collections.Generic;
using ChatApp.Shared.Misc;
using Cysharp.Threading.Tasks;
using ChatApp.Shared.Tables;

namespace ChatApp.Services
{
    public interface IFriendService
    {
        public List<UserSimple> Friends { get; set; }
        public List<UserSimple> FriendRequests { get; set; }
        public List<UserSimple> OutgoingFriendRequests { get; set; }
        
        public UniTask<(bool, string, UserSimple)> SendFriendRequest(string userName);
        public UniTask<(bool, string)> RespondToFriendRequest(string fromUserID, string toUserID, bool status, bool isCanceling = false);
        public event Action<UserSimple> OnFriendRequestReceived;
        public event Action<UserSimple> OnFriendRequestCanceled;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;
        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response);
        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification);
        public UniTask<bool> UpdateFriendsList();
        public void AddFriendToFriendsList(UserSimple user);
        public void CancelFriendRequest(UserSimple user);
        public UniTask<bool> GetFriendRequests();
    }
}