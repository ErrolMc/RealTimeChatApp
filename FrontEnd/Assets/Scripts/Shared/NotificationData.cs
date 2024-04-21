using System;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace ChatApp.Shared.Notifications
{
    public enum NotificationType
    {
        DirectMessage = 0,
        FriendRequest = 1,
        ServerMessage = 2,
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
        public string FromUserID { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
    }

    [Serializable]
    public class FriendRequestNotificationResponseData
    {
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}