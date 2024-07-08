using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Tables;

namespace ChatApp.Shared.Misc
{
    [Serializable]
    public class UserSimple
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        // profile image (id?)

        public UserSimple()
        {

        }

        public UserSimple(User user)
        {
            UserID = user.UserID;
            UserName = user.Username;
        }
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

    [Serializable]
    public class GenericResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}