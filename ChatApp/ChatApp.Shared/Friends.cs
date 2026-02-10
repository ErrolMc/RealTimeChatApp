using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ChatApp.Shared.TableDataSimple;

namespace ChatApp.Shared.Friends
{
    [Serializable]
    public class GetFriendsRequestData
    {
        public string UserID { get; set; }
        public int LocalVNum { get; set; }
    }
    
    [Serializable]
    public class RespondToFriendRequestData
    {
        public string ToUserID { get; set; }
        public string FromUserID { get; set; }
        public bool Status { get; set; }
        public bool isCanceling { get; set; }
    }
    
    [Serializable]
    public class RespondToFriendRequestResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    [Serializable]
    public class GetFriendsResponseData
    {
        public bool Success { get; set; }
        public bool HasUpdate { get; set; }
        public int VNum { get; set; }
        public string Message { get; set; }
        public List<UserSimple> Friends { get; set; } = new List<UserSimple>();
    }

    [Serializable]
    public class GetFriendRequestsResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<UserSimple> FriendRequests { get; set; } = new List<UserSimple>();
        public List<UserSimple> OutgoingFriendRequests { get; set; } = new List<UserSimple>();
    }
}