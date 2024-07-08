using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.TableDataSimple;

namespace ChatApp.Source.Services
{
    public interface IGroupService
    {
        public List<GroupDMSimple> GroupDMs { get; set; }
        
        public Task<(bool success, string message, GroupDMSimple groupDMSimple)> CreateGroupDM(List<string> friends);
        public Task<bool> UpdateGroupDMList();
        public void AddGroupLocally(GroupDMSimple groupDM);
        
        public event Action OnGroupDMsUpdated;
    }
}

