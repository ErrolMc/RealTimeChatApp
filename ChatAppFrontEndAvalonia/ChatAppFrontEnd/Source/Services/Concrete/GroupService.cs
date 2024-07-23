using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Utils;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class GroupService : IGroupService
    {
        private readonly IAuthenticationService _authenticationService;

        public event Action OnGroupDMsUpdated;
        public event Action<(GroupDMSimple groupDM, bool thisUserLeaving)> OnGroupUpdated;
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

            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message, null);
            }
            
            CreateGroupDMResponseData responseData = response.ResponseData;
            return (responseData.CreatedGroupSuccess, responseData.Message, responseData.GroupDMSimple);
        }
        
        public async Task<(bool success, string message)> RemoveUserFromGroup(string userID, GroupDMSimple groupDM, bool thisUserLeaving)
        {
            var requestData = new RemoveFromGroupRequestData()
            {
                UserID = userID,
                GroupID = groupDM.GroupID
            };
            
            var response =
                await NetworkHelper.PerformFunctionPostRequest<RemoveFromGroupRequestData, RemoveFromGroupResponseData>(FunctionNames.REMOVE_USER_FROM_GROUP, requestData);
            
            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message);
            }
            
            RemoveFromGroupResponseData responseData = response.ResponseData;

            if (responseData.Success)
            {
                groupDM.Name = responseData.GroupName;
                UpdateGroupLocally(groupDM, thisUserLeaving);
            }
            
            return (responseData.Success, responseData.Message);
        }

        public async Task<bool> UpdateGroupDMList()
        {
            UserSimple requestData = new UserSimple
            {   
                UserName = _authenticationService.CurrentUser.Username,
                UserID = _authenticationService.CurrentUser.UserID,
            };
            
            var response = await NetworkHelper.PerformFunctionPostRequest<UserSimple, GetGroupDMsResponseData>(FunctionNames.GET_GROUP_DMS, requestData);
            
            if (response.ConnectionSuccess == false)
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

        public void UpdateGroupLocally(GroupDMSimple groupDM, bool thisUserLeaving)
        {
            OnGroupUpdated?.Invoke((groupDM, thisUserLeaving));
        }
        
        public async Task<GetGroupParticipantsResponseData> GetGroupParticipants(string groupID)
        {
            var response = await NetworkHelper.PerformFunctionPostRequest<string, GetGroupParticipantsResponseData>(FunctionNames.GET_GROUP_PARTICIPANTS, groupID);
            
            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine($"GroupService - Failed to get groupDm participants, request failed: {response.Message}");
                return new GetGroupParticipantsResponseData() { Success = false, Message = "Request failed" };
            }
            
            if (response.ResponseData.Success == false)
            {
                Console.WriteLine($"GroupService - Failed to get groupDm participants: {response.ResponseData.Message}");
                return new GetGroupParticipantsResponseData() { Success = false, Message = response.ResponseData.Message };
            }

            return response.ResponseData;
        }
    }
}

