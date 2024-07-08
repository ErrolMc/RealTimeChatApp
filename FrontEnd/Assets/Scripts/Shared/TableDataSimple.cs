using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace ChatApp.Shared.TableDataSimple
{
    [Serializable]
    public class GroupDMSimple : IChatEntity
    {
        public string Name { get; set; }
        public string GroupID;
        
        public string ID 
        { 
            get => GroupID;
            set => GroupID = value;
        }
    }
    
    public interface IChatEntity
    {
        string ID { get; set; }
        string Name { get; set; }
    }
    
    [Serializable]
    public class UserSimple : IChatEntity
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        // profile image (id?)
        
        public string ID 
        { 
            get => UserID;
            set => UserID = value;
        }

        public string Name
        {
            get => UserName;
            set => UserName = value;
        }
    }
}