using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared;
using ChatApp.Shared.Enums;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class GroupService : IGroupService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICachingService _cachingService;
        private readonly INetworkCallerService _networkCaller;
        
        public event Action OnGroupDMsUpdated;
        public event Action<(GroupDMSimple groupDM, GroupUpdateReason reason)> OnGroupUpdated;
        public List<GroupDMSimple> GroupDMs { get; set; }
        
        public GroupService(IAuthenticationService authenticationService, ICachingService cachingService, INetworkCallerService networkCaller)
        {
            _authenticationService = authenticationService;
            _cachingService = cachingService;
            _networkCaller = networkCaller;
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
                await _networkCaller.PerformBackendPostRequest<CreateGroupDMRequestData, CreateGroupDMResponseData>(EndpointNames.CREATE_GROUP_DM, requestData);

            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message, null);
            }
            
            CreateGroupDMResponseData responseData = response.ResponseData;

            if (responseData.CreatedGroupSuccess)
            {
                AddGroupLocally(responseData.GroupDMSimple);
            }
            
            return (responseData.CreatedGroupSuccess, responseData.Message, responseData.GroupDMSimple);
        }

        public async Task<(bool success, string message, GroupDMSimple groupDMSimple)> AddFriendsToGroupDM(string groupID, List<string> friends)
        {
            var requestData = new AddFriendsToGroupDMRequestData()
            {
                GroupID = groupID,
                UsersToAdd = friends
            };
            
            var response =
                await _networkCaller.PerformBackendPostRequest<AddFriendsToGroupDMRequestData, AddFriendsToGroupDMResponseData>(EndpointNames.ADD_FRIENDS_TO_GROUP, requestData);
            
            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message, null);
            }

            AddFriendsToGroupDMResponseData responseData = response.ResponseData;

            if (responseData.Success)
            {
                UpdateGroupLocally(responseData.GroupDMSimple, GroupUpdateReason.DoesntMatter);
            }
            
            return (responseData.Success, responseData.Message, responseData.GroupDMSimple);
        }
        
        public async Task<(bool success, string message)> RemoveUserFromGroup(string userID, GroupDMSimple groupDM, GroupUpdateReason reason)
        {
            var requestData = new RemoveFromGroupRequestData()
            {
                UserID = userID,
                GroupID = groupDM.GroupID,
                Reason = reason
            };
            
            var response =
                await _networkCaller.PerformBackendPostRequest<RemoveFromGroupRequestData, RemoveFromGroupResponseData>(EndpointNames.REMOVE_USER_FROM_GROUP, requestData);
            
            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message);
            }
            
            RemoveFromGroupResponseData responseData = response.ResponseData;

            if (responseData.Success)
            {
                if (reason == GroupUpdateReason.UserLeft)
                    reason = GroupUpdateReason.ThisUserLeft;
                
                groupDM.Name = responseData.GroupName;
                UpdateGroupLocally(groupDM, reason);
            }
            
            return (responseData.Success, responseData.Message);
        }

        public async Task<(bool success, string message)> DeleteGroup(string groupID)
        {
            var response = await _networkCaller.PerformBackendPostRequest<string, DeleteGroupDMResponseData>(EndpointNames.DELETE_GROUP, groupID);
            
            if (response.ConnectionSuccess == false)
            {
                return (false, response.Message);
            }

            DeleteGroupDMResponseData responseData = response.ResponseData;

            if (responseData.Success)
            {
                UpdateGroupLocally(new GroupDMSimple() { GroupID = groupID}, GroupUpdateReason.GroupDeleted);
            }
            else
            {
                Console.WriteLine($"Delete Group DM Fail | Group Replace: {responseData.ReplaceGroupSuccess} User Replace: {responseData.ReplaceUserSuccess}");
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
            
            var response = await _networkCaller.PerformBackendPostRequest<UserSimple, GetGroupDMsResponseData>(EndpointNames.GET_GROUP_DMS, requestData);
            
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
            
            // update threads
            List<ThreadCache> threadsOnDisk = await _cachingService.GetAllThreads();
            HashSet<string> groupThreadsOnDisk = threadsOnDisk
                .Where(thread => (MessageType)thread.Type == MessageType.GroupMessage)
                .Select(thread => thread.ThreadID).ToHashSet();
            
            List<ThreadCache> threadsToAdd = new List<ThreadCache>();
            foreach (GroupDMSimple group in GroupDMs)
            {
                if (!groupThreadsOnDisk.Remove(group.GroupID))
                    threadsToAdd.Add(new ThreadCache() { ThreadID = group.GroupID, Type = (int)MessageType.GroupMessage, TimeStamp = 0 });
            }
            
            if (threadsToAdd.Count > 0)
                await _cachingService.AddThreads(threadsToAdd);
            
            if (groupThreadsOnDisk.Count > 0)
                await _cachingService.RemoveThreads(groupThreadsOnDisk.ToList());
            
            return true;
        }
        
        public void AddGroupLocally(GroupDMSimple groupDM)
        {
            GroupDMs?.Add(groupDM);
            OnGroupDMsUpdated?.Invoke();
            
            _cachingService.AddThreads(new List<ThreadCache>() { new() { ThreadID = groupDM.GroupID, Type = (int)MessageType.GroupMessage, TimeStamp = 0 } });
        }

        public void UpdateGroupLocally(GroupDMSimple groupDM, GroupUpdateReason reason)
        {
            if (reason.IsReasonToDeleteLocalGroup())
            {
                GroupDMSimple localGroupDM = GroupDMs.FirstOrDefault(gp => gp.GroupID == groupDM.GroupID);
                if (localGroupDM != null)
                {
                    GroupDMs?.Remove(localGroupDM);
                    _cachingService.RemoveThreads(new List<string>() { localGroupDM.GroupID });
                }
            }
            
            OnGroupUpdated?.Invoke((groupDM, reason));
        }
        
        public async Task<GetGroupParticipantsResponseData> GetGroupParticipants(string groupID)
        {
            var response = await _networkCaller.PerformBackendPostRequest<string, GetGroupParticipantsResponseData>(EndpointNames.GET_GROUP_PARTICIPANTS, groupID);
            
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
