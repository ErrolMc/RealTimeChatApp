using System;
using ChatApp.Shared.Notifications;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ChatApp.Services
{
    public interface IFriendService
    {
        public UniTask<(bool, string)> AddFriend(string userName);
        public List<FriendRequestNotification> ReceivedFriendRequestsThisSession { get; set; }
        public event Action<FriendRequestNotification> OnFriendRequestReceived;
        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification);
    }
}