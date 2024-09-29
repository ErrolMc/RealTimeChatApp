using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using LiteDB;

namespace ChatApp.Shared.TableDataSimple
{
    public interface IChatEntity
    {
        string ID { get; set; }
        string Name { get; set; }
        bool DoesMessageThreadMatch(MessageCache message);
    }
    
    [Serializable]
    public class GroupDMSimple : IChatEntity
    {
        public string Name { get; set; }
        public string Owner { get; set; }
        public string GroupID { get; set; }
        
        [JsonIgnore]
        public string ID 
        { 
            get => GroupID;
            set => GroupID = value;
        }
        
        public bool DoesMessageThreadMatch(MessageCache message)
        {
            return message.ThreadID == GroupID;
        }
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

        public bool DoesMessageThreadMatch(MessageCache message)
        {
            return message.FromUser.UserID == UserID;
        }
        
        [JsonIgnore]
        public string ID 
        { 
            get => UserID;
            set => UserID = value;
        }

        [JsonIgnore]
        public string Name
        {
            get => UserName;
            set => UserName = value;
        }
    }
}