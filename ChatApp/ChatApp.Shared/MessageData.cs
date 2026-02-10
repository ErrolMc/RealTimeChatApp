using System;
using System.Collections.Generic;
using ChatApp.Shared.Tables;

namespace ChatApp.Shared.Messages
{
    public enum MessageType
    {
        DirectMessage = 0,
        GroupMessage = 1,
        ServerMessage = 2,
    }

    [Serializable]
    public class SendMessageRequestData
    {
        public string ThreadID { get; set; }
        public string FromUserID { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        public string MetaData { get; set; } // other data thats required
        // attachments
    }

    [Serializable]
    public class SendMessageResponseData
    {
        public bool Success { get; set; }
        public bool NotificationSuccess { get; set; }
        public string ResponseMessage { get; set; }
        public Message Message { get; set; }
    }

    [Serializable]
    public class GetMessagesRequestData
    {
        public string ThreadID { get; set; }
        public long LocalTimeStamp { get; set; }
    }

    [Serializable]
    public class GetMessagesResponseData
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }
        public List<Message> Messages { get; set; }
    }
}