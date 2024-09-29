using System;
using ChatApp.Shared.TableDataSimple;
using LiteDB;

namespace ChatAppFrontEnd.Source.Other.Caching.Data
{
    [Serializable]
    public class StringWrapper
    {
        [BsonId]
        public string Key { get; set; }
        public string Value { get; set; }
    }

    [Serializable]
    public class ThreadCache
    {
        [BsonId]
        public string ThreadID { get; set; }
        public long TimeStamp { get; set; }
        public int Type { get; set; }
    }
    
    [Serializable]
    public class GroupDMCache
    {
        [BsonId]
        public string GroupID { get; set; }
        public string Owner { get; set; }
        public string Name { get; set; }
        public int MessageVNum { get; set; }
        public int MetaDataVNum { get; set; }
    }
    
    [Serializable]
    public class UserSimpleCache : UserSimple
    {
        public int MessageVNum { get; set; }

        public UserSimpleCache(UserSimple userSimple, int messageVNum = 0)
        {
            UserName = userSimple.UserName;
            UserID = userSimple.UserID;
            MessageVNum = messageVNum;
        }
    }
    
    [Serializable]
    public class MessageCache
    {
        [BsonId]
        public string MessageID { get; set; }
        public string ThreadID { get; set; }
        public UserSimple FromUser { get; set; }
        public string Message { get; set; }
        public long TimeStamp { get; set; }
    }
}
