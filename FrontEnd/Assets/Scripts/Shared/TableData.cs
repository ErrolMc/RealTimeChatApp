using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Misc;

namespace ChatApp.Shared.Tables
{
    [Serializable]
    public class User
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("userid")] public string UserID { get; set; }
        [JsonProperty("username")] public string Username { get; set; }
        [JsonProperty("hashedPassword")] public string HashedPassword { get; set; }
        [JsonProperty("isOnline")] public bool IsOnline { get; set; }
        [JsonProperty("rtChatConId")] public string RealTimeChatConnectionID { get; set; }
        [JsonProperty("friends")] public List<string> Friends { get; set; } = new List<string>();
        [JsonProperty("friendrequests")] public List<string> FriendRequests { get; set; } = new List<string>();
        [JsonProperty("outgoingfriendrequests")] public List<string> OutgoingFriendRequests { get; set; } = new List<string>();
        // profile image (id?)

        public UserSimple ToUserSimple()
        {
            return new UserSimple() { UserName = Username, UserID = UserID };
        }
    }
}
