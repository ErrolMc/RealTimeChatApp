using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;

namespace ChatAppFrontEnd.ViewModels.Logic
{
    public class SelectUsersLogic_AddToGroupDM : SelectUsersLogicBase
    {
        private readonly IGroupService _groupService;
        private readonly Action<GroupDMSimple> _onSuccessCallback;
        private readonly string _groupID;
        private readonly int _existingUsersInGroup;
        
        private GroupDMSimple _responseGroupDM;
        
        public SelectUsersLogic_AddToGroupDM(IGroupService groupService, string groupID, int existingUsersInGroup, Action<GroupDMSimple> onSuccessCallback)
        {
            _groupService = groupService;
            _onSuccessCallback = onSuccessCallback;
            _groupID = groupID;
            _existingUsersInGroup = existingUsersInGroup;
            
            _responseGroupDM = null;
        }
        
        public override async Task<(bool result, string message)> HandleConfirm(List<string> userIDs)
        {
            var response = await _groupService.AddFriendsToGroupDM(_groupID, userIDs);

            if (response.success)
                _responseGroupDM = response.groupDMSimple;
            
            return (response.success, response.message);
        }

        public override void OnSuccess()
        {
            _onSuccessCallback?.Invoke(_responseGroupDM);
        }
        
        public override int MaxAmount => SelectUsersLogic_CreateGroupDM.MAX_USERS_IN_GROUP;
        public override int AmountDifference => _existingUsersInGroup;
    }
}

