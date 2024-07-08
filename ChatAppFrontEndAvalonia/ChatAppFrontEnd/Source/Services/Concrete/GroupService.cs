using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Utils;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class GroupService : IGroupService
    {
        private readonly IAuthenticationService _authenticationService;

        public event Action OnGroupDMsUpdated;
        public List<GroupDMSimple> GroupDMs { get; set; }
        
        public GroupService(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            GroupDMs = new List<GroupDMSimple>();
        }

        public async Task<(bool success, string message, GroupDMSimple groupDMSimple)> CreateGroupDM(List<string> friends)
        {
            string curUserID = _authenticationService.CurrentUser.UserID;
            friends.Add(curUserID);
            
            var requestData = new CreateGroupDMRequestData()
            {
                Creator = curUserID,
                Participants = friends
            };

            var response =
                await NetworkHelper.PerformFunctionPostRequest<CreateGroupDMRequestData, CreateGroupDMResponseData>(FunctionNames.CREATE_GROUP_DM, requestData);

            CreateGroupDMResponseData responseData = response.ResponseData;
            return (responseData.CreatedGroupSuccess, response.Message, responseData.GroupDMSimple);
        }

        public async Task<bool> UpdateGroupDMList()
        {
            UserSimple requestData = new UserSimple
            {   
                UserName = _authenticationService.CurrentUser.Username,
                UserID = _authenticationService.CurrentUser.UserID,
            };
            
            var response = await NetworkHelper.PerformFunctionPostRequest<UserSimple, GetGroupDMsResponseData>(FunctionNames.GET_GROUP_DMS, requestData);
            
            if (response.Success == false)
            {
                Console.WriteLine($"GroupService - Failed to get groupDms, request failed: {response.Message}");
                return false;
            }
            
            if (response.ResponseData.Success == false)
            {
                Console.WriteLine($"GroupService - Failed to get groupDms: {response.ResponseData.Message}");
                return false;
            }

            GroupDMs = response.ResponseData.GroupDMs;
            
            return true;
        }
        
        public void AddGroupLocally(GroupDMSimple groupDM)
        {
            GroupDMs?.Add(groupDM);
            OnGroupDMsUpdated?.Invoke();
        }
    }
}

