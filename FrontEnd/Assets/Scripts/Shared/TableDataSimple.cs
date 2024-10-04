using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Tables;
#if FRONTEND
using ChatAppFrontEnd.Source.Other.Caching.Data;
using LiteDB;
#endif

namespace ChatApp.Shared.TableDataSimple
{
    public interface IChatEntity
    {
        string ID { get; set; }
        string Name { get; set; }
#if FRONTEND
        bool DoesMessageThreadMatch(MessageCache message);
#endif
    }
    
    [Serializable]
    public class GroupDMSimple : IChatEntity
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string GroupID { get; set; }

#if FRONTEND
        [BsonIgnore]
#endif
        [JsonIgnore]
        public string ID 
        { 
            get => GroupID;
            set => GroupID = value;
        }
        
#if FRONTEND
        public bool DoesMessageThreadMatch(MessageCache message)
        {
            return message.ThreadID == GroupID;
        }
#endif
    }
    
    [Serializable]
    public class GroupDMSimpleCache : GroupDMSimple
    {
        public int MessageVNum { get; set; }

        public GroupDMSimpleCache(GroupDMSimpleCache groupDmSimple, int messageVNum = 0)
        {
            Name = groupDmSimple.Name;
            Owner = groupDmSimple.Owner;
            GroupID = groupDmSimple.GroupID;
            MessageVNum = messageVNum;
        }
    }
    
    [Serializable]
    public class UserSimple : IChatEntity
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        // profile image (id?)

#if FRONTEND
        public bool DoesMessageThreadMatch(MessageCache message)
        {
            return message.FromUser.UserID == UserID;
        }
#endif

#if FRONTEND
        [BsonIgnore]
#endif
        [JsonIgnore]
        public string ID 
        { 
            get => UserID;
            set => UserID = value;
        }

#if FRONTEND
        [BsonIgnore]
#endif
        [JsonIgnore]
        public string Name
        {
            get => UserName;
            set => UserName = value;
        }
    }
}