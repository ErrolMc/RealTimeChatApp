using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Misc;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.TableDataSimple;

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
        [JsonProperty("groupdms")] public List<string> GroupDMs { get; set; } = new List<string>();
        // profile image (id?)

        public UserSimple ToUserSimple()
        {
            return new UserSimple() { UserName = Username, UserID = UserID };
        }
    }

    [Serializable]
    public class Message
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("threadid")] public string ThreadID { get; set; }
        [JsonProperty("fromuser")] public UserSimple FromUser { get; set; }
        [JsonProperty("messagecontents")] public string MessageContents { get; set; }
        [JsonProperty("messagetype")] public int MessageType { get; set; }
        [JsonProperty("timestamp")] public long TimeStamp { get; set; }
        // attachments ?
    }

    [Serializable]
    public class GroupDM
    {
        [JsonProperty("id")] public string ID { get; set; }
        [JsonProperty("threadid")] public string ThreadID { get; set; }
        [JsonProperty("owner")] public string OwnerUserID { get; set; }
        [JsonProperty("participants")] public List<string> ParticipantUserIDs { get; set; } = new List<string>();
        [JsonProperty("hascustomname")] public bool HasCustomName { get; set; }
        [JsonProperty("name")] public string Name { get; set; }

        public GroupDMSimple ToGroupDMSimple()
        {
            return new GroupDMSimple() { Name = Name, GroupID = ID };
        }
    }
}
