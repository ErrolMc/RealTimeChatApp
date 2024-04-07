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
        [JsonProperty("A")] public string UserID { get; set; }
        [JsonProperty("B")] public string UserName { get; set; }
    }

    [Serializable]
    public class AuthenticateResponseData
    {
        [JsonProperty("A")] public bool Status { get; set; }
        [JsonProperty("B")] public string Message { get; set; }
        [JsonProperty("C")] public string AccessToken { get; set; }
    }

    [Serializable]
    public class NotificationData
    {
        [JsonProperty("A")] public int NotificationTypeInt { get; set; }
        [JsonProperty("B")] public string RecipientUserID { get; set; }
        [JsonProperty("C")] public string NotificationJson { get; set; }

        public NotificationType NotificationType
        {
            get => (NotificationType)NotificationTypeInt;
            set => NotificationTypeInt = (int)value;
        }
    }

    [Serializable]
    public class FriendRequestNotification
    {
        [JsonProperty("A")] public string FromUserID { get; set; }
        [JsonProperty("B")] public string FromUserName { get; set; }
        [JsonProperty("C")] public string ToUserName { get; set; }
    }

    [Serializable]
    public class FriendRequestNotificationResponseData
    {
        [JsonProperty("A")] public bool Status { get; set; }
        [JsonProperty("B")] public string Message { get; set; }
    }
}