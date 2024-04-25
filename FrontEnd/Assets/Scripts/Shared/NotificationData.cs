using System;
using ChatApp.Shared.Misc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ChatApp.Shared.Notifications
{
    public enum NotificationType
    {
        DirectMessage = 0, 
        FriendRequest = 1,
        ServerMessage = 2,
        FriendRequestRespond = 3, // telling a sender the result of their sent friend request
        FriendRequestCancel = 4, // telling the recipient of a friend request it was cancelled
    }
    
    [Serializable]
    public class AuthenticateRequestData
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
    }

    [Serializable]
    public class AuthenticateResponseData
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public string AccessToken { get; set; }
    }

    [Serializable]
    public class NotificationData
    {
        public int NotificationType { get; set; }
        public string RecipientUserID { get; set; }
        public string NotificationJson { get; set; }
    }

    [Serializable]
    public class FriendRequestNotification
    {
        public UserSimple FromUser { get; set; }
        public string ToUserName { get; set; }
    }

    [Serializable]
    public class FriendRequestRespondNotification
    {
        public UserSimple ToUser { get; set; }
        public bool Status { get; set; }
    }

    [Serializable]
    public class FriendRequestNotificationResponseData
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public UserSimple ToUser { get; set; }
    }
}