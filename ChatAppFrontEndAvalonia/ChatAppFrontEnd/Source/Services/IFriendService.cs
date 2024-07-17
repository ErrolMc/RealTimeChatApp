using System;
using ChatApp.Shared.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;

namespace ChatApp.Services
{
    public interface IFriendService
    {
        public List<UserSimple> Friends { get; set; }
        public List<UserSimple> FriendRequests { get; set; }
        public List<UserSimple> OutgoingFriendRequests { get; set; }
        
        public event Action<UserSimple> OnFriendRequestReceived;
        public event Action<UserSimple> OnFriendRequestCanceled;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;
        public event Action<UnfriendNotification> OnUnfriended;
        public event Action FriendsListUpdated;
        
        public Task<(bool success, string message, UserSimple user)> SendFriendRequest(string userName);
        public Task<(bool success, string message)> RespondToFriendRequest(string fromUserID, string toUserID, bool status, bool isCanceling = false);
        
        public Task<(bool success, string message)> UnFriend(string toUserID);

        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response);
        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification);
        public Task<bool> UpdateFriendsList();
        public void AddFriendToLocalFriendsList(UserSimple user);
        public void OnCancelFriendRequest(UserSimple user);
        public void OnUnfriend(UnfriendNotification notification);
        public Task<bool> GetFriendRequests();
    }
}