using System;
using System.Collections;
using System.Collections.Generic;
using ChatApp.Shared.Enums;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using Newtonsoft.Json;

namespace ChatApp.Shared.GroupDMs
{
    [Serializable]
    public class CreateGroupDMRequestData
    {
        public string Creator { get; set; }
        public List<string> Participants { get; set; } = new List<string>();
    }
    
    [Serializable]
    public class CreateGroupDMResponseData
    {
        public bool CreatedGroupSuccess { get; set; }
        public bool UpdateDatabaseSuccess { get; set; }
        public string Message { get; set; }
        public GroupDMSimple GroupDMSimple { get; set; }
    }

    [Serializable]
    public class GetGroupDMsResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<GroupDMSimple> GroupDMs { get; set; } = new List<GroupDMSimple>();
    }

    [Serializable]
    public class GetGroupParticipantsResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string OwnerUserID { get; set; }
        public List<UserSimple> Participants { get; set; } = new List<UserSimple>();
    }

    [Serializable]
    public class RemoveFromGroupRequestData
    {
        public string UserID { get; set; }
        public string GroupID { get; set; }
        public GroupUpdateReason Reason { get; set; }
    }
    
    [Serializable]
    public class RemoveFromGroupResponseData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string GroupName { get; set; }
    }
}
