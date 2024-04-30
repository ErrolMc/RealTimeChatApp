using System;
using System.Collections.Generic;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;

namespace ChatApp.Shared.Messages
{
    [Serializable]
    public class SaveMessageRequestData
    {
        public string ThreadID { get; set; }
        public string FromUserID { get; set; }
        public string Message { get; set; }
        public int MessageType { get; set; }
        // attachments
    }

    [Serializable]
    public class SaveMessageResponseData
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }
        public Message Message { get; set; }
    }

    [Serializable]
    public class GetMessagesRequestData
    {
        public string ThreadID { get; set; }
    }

    [Serializable]
    public class GetMessagesResponseData
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }
        public List<Message> Messages { get; set; }
    }
}