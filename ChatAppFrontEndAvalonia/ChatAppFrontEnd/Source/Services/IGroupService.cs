using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;

namespace ChatApp.Source.Services
{
    public interface IGroupService
    {
        public List<GroupDMSimple> GroupDMs { get; set; }
        
        public event Action OnGroupDMsUpdated;
        public event Action<(GroupDMSimple groupDM, bool thisUserLeaving)> OnGroupUpdated;
        
        public Task<(bool success, string message)> RemoveUserFromGroup(string userID, GroupDMSimple groupDM, bool thisUserLeaving);
        public Task<(bool success, string message, GroupDMSimple groupDMSimple)> CreateGroupDM(List<string> friends);
        public Task<GetGroupParticipantsResponseData> GetGroupParticipants(string groupID);
        
        public Task<bool> UpdateGroupDMList();
        public void AddGroupLocally(GroupDMSimple groupDM);
        public void UpdateGroupLocally(GroupDMSimple groupDM, bool thisUserLeaving);
    }
}

