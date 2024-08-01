using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;

namespace ChatAppFrontEnd.ViewModels.Logic
{
    public class SelectUsersLogic_CreateGroupDM : SelectUsersLogicBase
    {
        public const int MAX_USERS_IN_GROUP = 10;
        
        private readonly IGroupService _groupService;
        private readonly Action<GroupDMSimple> _onSuccessCallback;

        private GroupDMSimple _responseGroupDM;
        
        public SelectUsersLogic_CreateGroupDM(IGroupService groupService, Action<GroupDMSimple> onSuccessCallback)
        {
            _groupService = groupService;
            _onSuccessCallback = onSuccessCallback;
            _responseGroupDM = null;
        }
        
        public override async Task<(bool result, string message)> HandleConfirm(List<string> userIDs)
        {
            var response = await _groupService.CreateGroupDM(userIDs);

            if (response.success)
                _responseGroupDM = response.groupDMSimple;
            
            return (response.success, response.message);
        }

        public override void OnSuccess()
        {
            _onSuccessCallback?.Invoke(_responseGroupDM);
        }

        public override int MaxAmount => MAX_USERS_IN_GROUP;
        public override int AmountDifference => 0;
    }
}

