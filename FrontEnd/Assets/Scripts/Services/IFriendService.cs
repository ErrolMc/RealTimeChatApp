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
        public UniTask<(bool, string)> AddFriend(string userName);
        public UniTask<(bool, string)> RespondToFriendRequest(string fromUserID, bool status);
        public List<FriendRequestNotification> ReceivedFriendRequestsThisSession { get; set; }
        public event Action<FriendRequestNotification> OnFriendRequestReceived;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;
        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response);
        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification);
        public UniTask<bool> UpdateFriendsList();
        public void AddFriendToFriendsList(UserSimple user);
        public void RespondToFriendRequest(FriendRequestNotification notification, bool status);
    }
}