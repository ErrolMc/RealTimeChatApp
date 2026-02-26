using ChatApp.Shared.GroupDMs;
using Microsoft.Azure.Cosmos;
using ChatApp.Shared.Tables;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;
using ChatApp.Shared.ExtensionMethods;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Enums;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Repositories
{
    public class GroupsRepository
    {
        private readonly DatabaseService _db;
        private readonly QueryService _queries;

        public GroupsRepository(DatabaseService db, QueryService queries)
        {
            _db = db;
            _queries = queries;
        }

        public async Task<(CreateGroupDMResponseData result, List<string> notifyUserIds)> CreateGroupDM(CreateGroupDMRequestData requestData)
        {
            var getParticipantsResp = await _queries.GetUsers(requestData.Participants);
            if (getParticipantsResp.IsSuccessful == false)
                return (new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get participant user info from database" }, new List<string>());

            List<User> participants = getParticipantsResp.Data;
            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, requestData.Creator);

            if (!ownerFound)
                return (new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Couldn't get creator user info from database" }, new List<string>());

            string threadID = Guid.NewGuid().ToString();
            ChatThread groupDM = new ChatThread()
            {
                ID = threadID,
                IsGroupDM = true,
                MessageVNum = 0,
                OwnerUserID = requestData.Creator,
                Users = requestData.Participants,
                Name = participants.GetGroupName()
            };

            try
            {
                await _db.ChatThreadsContainer.CreateItemAsync(groupDM, new PartitionKey(threadID));
            }
            catch (Exception)
            {
                return (new CreateGroupDMResponseData { CreatedGroupSuccess = false, UpdateDatabaseSuccess = false, Message = "Database error" }, new List<string>());
            }

            List<string> notifyUserIds = new List<string>();
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in participants)
            {
                if (!user.GroupDMs.Contains(threadID))
                    user.GroupDMs.Add(threadID);

                var replaceResponse = await _db.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    failedDatabaseUpdates.Add(user.UserID);
                    continue;
                }

                if (user.UserID == owner.UserID)
                    continue;

                notifyUserIds.Add(user.UserID);
            }

            if (failedDatabaseUpdates.Count > 0)
                return (new CreateGroupDMResponseData() { CreatedGroupSuccess = true, UpdateDatabaseSuccess = false, Message = $"Successfully created group! Coundn't update database for {failedDatabaseUpdates.Count}/{groupDM.Users.Count} users", GroupDMSimple = groupDM.ToGroupDMSimple() }, notifyUserIds);

            return (new CreateGroupDMResponseData() { CreatedGroupSuccess = true, UpdateDatabaseSuccess = true, Message = $"Successfully created group!", GroupDMSimple = groupDM.ToGroupDMSimple() }, notifyUserIds);
        }

        public virtual async Task<GetGroupDMsResponseData> GetGroupDMs(UserSimple requestData)
        {
            var userResp = await _queries.GetUserFromUserID(requestData.UserID);
            if (userResp.IsSuccessful == false)
                return new GetGroupDMsResponseData { Success = false, Message = userResp.ErrorMessage };

            var groupDMResp = await _queries.GetChatThreadsFromIDs(userResp.Data.GroupDMs);
            if (groupDMResp.IsSuccessful == false)
                return new GetGroupDMsResponseData { Success = false, Message = groupDMResp.ErrorMessage };

            List<GroupDMSimple> groupDMSimples = groupDMResp.Data.ToGroupDMSimpleList();
            return new GetGroupDMsResponseData() { Success = true, Message = $"Gathered {groupDMSimples.Count} group dms", GroupDMs = groupDMSimples };
        }

        public async Task<GetGroupParticipantsResponseData> GetGroupParticipants(string groupID)
        {
            var groupDMResp = await _queries.GetChatThreadFromThreadID(groupID);
            if (groupDMResp.IsSuccessful == false)
                return new GetGroupParticipantsResponseData { Success = false, Message = groupDMResp.ErrorMessage };

            ChatThread groupDM = groupDMResp.Data;
            var participantResp = await _queries.GetUsers(groupDM.Users);
            if (participantResp.IsSuccessful == false)
                return new GetGroupParticipantsResponseData { Success = false, Message = participantResp.ErrorMessage };

            return new GetGroupParticipantsResponseData { Success = true, Message = "Success", OwnerUserID = groupDM.OwnerUserID, Participants = participantResp.Data.ToUserSimpleList() };
        }

        public async Task<(AddFriendsToGroupDMResponseData result, List<string> addedUserIds, List<string> updatedUserIds)> AddFriendsToGroup(AddFriendsToGroupDMRequestData requestData)
        {
            var groupDMResp = await _queries.GetChatThreadFromThreadID(requestData.GroupID);
            if (groupDMResp.IsSuccessful == false)
                return (new AddFriendsToGroupDMResponseData { Success = false, Message = $"Couldn't find group with id {requestData.GroupID}" }, new List<string>(), new List<string>());

            ChatThread groupDM = groupDMResp.Data;
            HashSet<string> usersToAdd = requestData.UsersToAdd.ToHashSet();

            foreach (string userID in requestData.UsersToAdd)
            {
                if (groupDM.Users.Contains(userID))
                    usersToAdd.Remove(userID);
                else
                    groupDM.Users.Add(userID);
            }

            var getUserResp = await _queries.GetUsers(groupDM.Users);
            if (getUserResp.IsSuccessful == false)
                return (new AddFriendsToGroupDMResponseData { Success = false, Message = "Couldn't get participant user info from database" }, new List<string>(), new List<string>());

            List<User> participants = getUserResp.Data;
            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, groupDM.OwnerUserID);
            if (!ownerFound)
                return (new AddFriendsToGroupDMResponseData { Success = false, Message = "Couldn't get creator user info from database" }, new List<string>(), new List<string>());

            groupDM.Name = participants.GetGroupName();

            try
            {
                var groupReplaceResponse = await _db.ChatThreadsContainer.ReplaceItemAsync(groupDM, groupDM.ID, new PartitionKey(groupDM.ID));
                if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    return (new AddFriendsToGroupDMResponseData { Success = false, Message = $"Couldn't replace group {requestData.GroupID}" }, new List<string>(), new List<string>());
            }
            catch (Exception ex)
            {
                return (new AddFriendsToGroupDMResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" }, new List<string>(), new List<string>());
            }

            List<string> addedUserIds = new List<string>();
            List<string> updatedUserIds = new List<string>();
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in participants)
            {
                if (user.UserID == groupDM.OwnerUserID) continue;

                bool isNewUser = usersToAdd.Contains(user.ID);
                if (isNewUser)
                {
                    if (!user.GroupDMs.Contains(groupDM.ID))
                        user.GroupDMs.Add(groupDM.ID);

                    var replaceResponse = await _db.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                    if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        failedDatabaseUpdates.Add(user.UserID);
                        continue;
                    }

                    addedUserIds.Add(user.UserID);
                }
                else
                {
                    updatedUserIds.Add(user.UserID);
                }
            }

            if (failedDatabaseUpdates.Count > 0)
                return (new AddFriendsToGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = false, Message = $"Successfully updated group after adding users! Coundn't update database for {failedDatabaseUpdates.Count}/{usersToAdd.Count} users", GroupDMSimple = groupDM.ToGroupDMSimple() }, addedUserIds, updatedUserIds);

            return (new AddFriendsToGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = true, Message = $"Successfully added users to group!", GroupDMSimple = groupDM.ToGroupDMSimple() }, addedUserIds, updatedUserIds);
        }

        public async Task<(RemoveFromGroupResponseData result, List<string> remainingParticipantIds, string ownerUserID, GroupDMSimple groupDMSimple)> RemoveUserFromGroup(RemoveFromGroupRequestData requestData)
        {
            var groupDMResp = await _queries.GetChatThreadFromThreadID(requestData.GroupID);
            if (groupDMResp.IsSuccessful == false)
                return (new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't find group with id {requestData.GroupID}" }, new List<string>(), null, null);

            var getParticipantsResp = await _queries.GetUsers(groupDMResp.Data.Users);
            if (getParticipantsResp.IsSuccessful == false)
                return (new RemoveFromGroupResponseData { Success = false, Message = "Couldn't get participant user info from database" }, new List<string>(), null, null);

            ChatThread groupDM = groupDMResp.Data;
            List<User> participants = getParticipantsResp.Data;
            User userToRemove = participants.FirstOrDefault(u => u.UserID == requestData.UserID);

            if (userToRemove == null)
                return (new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't find user {requestData.UserID} in group {requestData.GroupID}" }, new List<string>(), null, null);

            bool updatedGroup = groupDM.Users.Remove(requestData.UserID);
            bool updatedUser = userToRemove.GroupDMs.Remove(requestData.GroupID);

            if (!updatedGroup && !updatedUser)
                return (new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't remove user {userToRemove.UserID} from group {groupDM.ID}, UpdateUser: {updatedUser}, UpdateGroup: {updatedGroup}" }, new List<string>(), null, null);

            bool ownerFound = participants.GetOwnerAndPutAtFront(out User owner, groupDM.OwnerUserID);
            if (!ownerFound)
                return (new RemoveFromGroupResponseData { Success = false, Message = "Couldn't get creator user info from database" }, new List<string>(), null, null);

            if (participants.Remove(userToRemove))
            {
                updatedGroup = true;
                groupDM.Name = participants.GetGroupName();
            }

            if (updatedGroup)
            {
                try
                {
                    var groupReplaceResponse = await _db.ChatThreadsContainer.ReplaceItemAsync(groupDM, groupDM.ID, new PartitionKey(groupDM.ID));
                    if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                        return (new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't replace group {requestData.GroupID} after removing user {requestData.UserID}" }, new List<string>(), null, null);
                }
                catch (Exception ex)
                {
                    return (new RemoveFromGroupResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" }, new List<string>(), null, null);
                }
            }

            if (updatedUser)
            {
                try
                {
                    var userReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(userToRemove, userToRemove.UserID, new PartitionKey(userToRemove.UserID));
                    if (userReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                        return (new RemoveFromGroupResponseData { Success = false, Message = $"Couldn't replace user {requestData.UserID} after removing group {requestData.GroupID}" }, new List<string>(), null, null);
                }
                catch (Exception ex)
                {
                    return (new RemoveFromGroupResponseData { Success = false, Message = $"Group Replace Exception: {ex.Message}" }, new List<string>(), null, null);
                }
            }

            List<string> remainingParticipantIds = participants.Select(u => u.UserID).ToList();

            return (new RemoveFromGroupResponseData() { Success = true, Message = "Successfully removed user from group!", GroupName = groupDM.Name }, remainingParticipantIds, groupDM.OwnerUserID, groupDM.ToGroupDMSimple());
        }

        public async Task<(DeleteGroupDMResponseData result, List<string> notifyUserIds, GroupDMSimple groupDMSimple)> DeleteGroup(string groupID)
        {
            var groupDMResp = await _queries.GetChatThreadFromThreadID(groupID);
            if (groupDMResp.IsSuccessful == false)
                return (new DeleteGroupDMResponseData { Success = false, Message = $"Couldn't find group with id {groupID}" }, new List<string>(), null);

            ChatThread groupDM = groupDMResp.Data;

            var getParticipantsResp = await _queries.GetUsers(groupDM.Users);
            if (getParticipantsResp.IsSuccessful == false)
                return (new DeleteGroupDMResponseData { Success = false, Message = "Couldn't get participant user info from database" }, new List<string>(), null);

            try
            {
                var groupReplaceResponse = await _db.ChatThreadsContainer.DeleteItemAsync<ChatThread>(groupDM.ID, new PartitionKey(groupDM.ID));
                if (groupReplaceResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
                    return (new DeleteGroupDMResponseData { Success = false, Message = $"Couldn't delete group {groupID}" }, new List<string>(), null);
            }
            catch (Exception ex)
            {
                return (new DeleteGroupDMResponseData { Success = false, Message = $"Group delete Exception: {ex.Message}" }, new List<string>(), null);
            }

            await _queries.DeleteMessagesByThreadID(groupDM.ID);

            List<string> notifyUserIds = new List<string>();
            List<string> failedDatabaseUpdates = new List<string>();
            foreach (User user in getParticipantsResp.Data)
            {
                user.GroupDMs.Remove(groupID);
                var replaceResponse = await _db.UsersContainer.ReplaceItemAsync(user, user.UserID, new PartitionKey(user.UserID));
                if (replaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    failedDatabaseUpdates.Add(user.UserID);
                    continue;
                }

                if (user.UserID == groupDM.OwnerUserID) continue;

                notifyUserIds.Add(user.UserID);
            }

            GroupDMSimple groupDMSimple = groupDM.ToGroupDMSimple();

            if (failedDatabaseUpdates.Count > 0)
                return (new DeleteGroupDMResponseData() { Success = false, ReplaceGroupSuccess = true, ReplaceUserSuccess = false, Message = $"Successfully deleted group! Coundn't update database for {failedDatabaseUpdates.Count}/{groupDM.Users.Count} users" }, notifyUserIds, groupDMSimple);

            return (new DeleteGroupDMResponseData() { Success = true, ReplaceGroupSuccess = true, ReplaceUserSuccess = true, Message = $"Successfully deleted group!" }, notifyUserIds, groupDMSimple);
        }
    }
}
