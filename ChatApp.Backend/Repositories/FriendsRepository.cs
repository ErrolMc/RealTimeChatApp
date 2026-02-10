using ChatApp.Shared.Notifications;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Misc;
using Microsoft.Azure.Cosmos;
using User = ChatApp.Shared.Tables.User;
using ChatApp.Shared;
using ChatApp.Shared.ExtensionMethods;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Friends;
using ChatApp.Shared.Tables;
using ChatApp.Backend.Services;

namespace ChatApp.Backend.Repositories
{
    public class FriendsRepository
    {
        private readonly DatabaseService _db;
        private readonly QueryService _queries;

        public FriendsRepository(DatabaseService db, QueryService queries)
        {
            _db = db;
            _queries = queries;
        }

        public async Task<FriendRequestNotificationResponseData> SendFriendRequest(FriendRequestNotification requestData)
        {
            var toUserResp = await _queries.GetUserFromUsername(requestData.ToUserName);
            if (toUserResp.IsSuccessful == false)
                return new FriendRequestNotificationResponseData { Status = false, Message = toUserResp.ErrorMessage };

            var fromUserResp = await _queries.GetUserFromUserID(requestData.FromUser.UserID);
            if (fromUserResp.IsSuccessful == false)
                return new FriendRequestNotificationResponseData { Status = false, Message = fromUserResp.ErrorMessage };

            User toUser = toUserResp.Data;
            User fromUser = fromUserResp.Data;

            if (fromUser.Friends.Contains(toUser.UserID))
                return new FriendRequestNotificationResponseData { Status = false, Message = "The 2 users are already friends!" };

            if (toUser.FriendRequests.Contains(fromUser.UserID) || fromUser.OutgoingFriendRequests.Contains(toUser.UserID))
                return new FriendRequestNotificationResponseData { Status = false, Message = "Friend request already sent to this user!" };

            toUser.FriendRequests.Add(fromUser.UserID);
            fromUser.OutgoingFriendRequests.Add(toUser.UserID);

            try
            {
                var toUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    return new FriendRequestNotificationResponseData { Status = false, Message = $"Couldnt get users from database - ToUser: {toUser.UserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {fromUser.UserID} Status: {fromUserReplaceResponse.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new FriendRequestNotificationResponseData { Status = false, Message = $"SendFriendRequest Replace Exception: {ex.Message}" };
            }

            return new FriendRequestNotificationResponseData { Status = true, Message = "Friend request sent successfully", ToUser = toUser.ToUserSimple() };
        }

        public async Task<(RespondToFriendRequestResponseData result, UserSimple fromUser, UserSimple toUser)> RespondToFriendRequest(RespondToFriendRequestData requestData)
        {
            var toUserResp = await _queries.GetUserFromUserID(requestData.ToUserID);
            if (toUserResp.IsSuccessful == false)
                return (new RespondToFriendRequestResponseData { Success = false, Message = toUserResp.ErrorMessage }, null, null);

            var fromUserResp = await _queries.GetUserFromUserID(requestData.FromUserID);
            if (fromUserResp.IsSuccessful == false)
                return (new RespondToFriendRequestResponseData { Success = false, Message = fromUserResp.ErrorMessage }, null, null);

            User toUser = toUserResp.Data;
            User fromUser = fromUserResp.Data;

            if (toUser == null || fromUser == null)
                return (new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} IsNull: {toUser == null} FromUser: {requestData.FromUserID} IsNull: {fromUser == null}" }, null, null);

            ChatThread thread = new ChatThread()
            {
                ID = SharedStaticMethods.CreateHashedDirectMessageID(fromUser.UserID, toUser.UserID),
                IsGroupDM = false,
                Users = new List<string> { fromUser.UserID, toUser.UserID },
                OwnerUserID = null,
                Name = null,
            };

            try
            {
                await _db.ChatThreadsContainer.CreateItemAsync(thread, new PartitionKey(thread.ID));
            }
            catch (Exception)
            {
                return (new RespondToFriendRequestResponseData { Success = false, Message = "Error when creating thread" }, null, null);
            }

            toUser.FriendRequests.Remove(fromUser.UserID);
            fromUser.OutgoingFriendRequests.Remove(toUser.UserID);

            if (requestData.Status)
            {
                if (!toUser.Friends.Contains(fromUser.UserID))
                {
                    toUser.Friends.Add(fromUser.UserID);
                    toUser.FriendsVNum++;
                }
                if (!fromUser.Friends.Contains(toUser.UserID))
                {
                    fromUser.Friends.Add(toUser.UserID);
                    fromUser.FriendsVNum++;
                }
            }

            try
            {
                var toUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    return (new RespondToFriendRequestResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {requestData.FromUserID} Status: {fromUserReplaceResponse.StatusCode}" }, null, null);
            }
            catch (Exception ex)
            {
                return (new RespondToFriendRequestResponseData { Success = false, Message = $"RespondToFriendRequest Replace Exception: {ex.Message}" }, null, null);
            }

            return (new RespondToFriendRequestResponseData { Success = true, Message = "Successfully responded to friend request" }, fromUser.ToUserSimple(), toUser.ToUserSimple());
        }

        public async Task<GetFriendsResponseData> GetFriends(GetFriendsRequestData requestData)
        {
            var userResp = await _queries.GetUserFromUserID(requestData.UserID);
            if (userResp.IsSuccessful == false)
                return new GetFriendsResponseData { Success = false, HasUpdate = false, VNum = -1, Message = userResp.ErrorMessage };

            if (userResp.Data.FriendsVNum == requestData.LocalVNum)
                return new GetFriendsResponseData { Success = true, HasUpdate = false, VNum = -1, Message = "Friends list up to date" };

            if (userResp.Data.Friends == null || userResp.Data.Friends.Count == 0)
                return new GetFriendsResponseData { Success = true, HasUpdate = true, VNum = userResp.Data.FriendsVNum, Message = "No friends found" };

            Result<List<User>> friendsResult = await _queries.GetUsers(userResp.Data.Friends);

            if (friendsResult.IsSuccessful == false)
            {
                Console.WriteLine($"An error occurred: {friendsResult.ErrorMessage}");
                return new GetFriendsResponseData { Success = false, HasUpdate = false, VNum = -1, Message = "An error occurred while getting friends" };
            }

            return new GetFriendsResponseData { Success = true, HasUpdate = true, Message = $"{friendsResult.Data.Count} Friends retrieved", VNum = userResp.Data.FriendsVNum, Friends = friendsResult.Data.ToUserSimpleList() };
        }

        public async Task<GetFriendRequestsResponseData> GetFriendRequests(UserSimple requestData)
        {
            var userResp = await _queries.GetUserFromUserID(requestData.UserID);

            if (userResp.IsSuccessful == false)
                return new GetFriendRequestsResponseData { Success = false, Message = userResp.ErrorMessage };

            var reqResp = await _queries.GetUsers(userResp.Data.FriendRequests);
            var outResp = await _queries.GetUsers(userResp.Data.OutgoingFriendRequests);

            if (reqResp.IsException)
                return new GetFriendRequestsResponseData { Success = false, Message = reqResp.ErrorMessage };

            if (outResp.IsException)
                return new GetFriendRequestsResponseData { Success = false, Message = outResp.ErrorMessage };

            return new GetFriendRequestsResponseData { Success = true, Message = "Success", FriendRequests = reqResp.Data.ToUserSimpleList(), OutgoingFriendRequests = outResp.Data.ToUserSimpleList() };
        }

        public async Task<GenericResponseData> RemoveFriend(UnfriendNotification requestData)
        {
            var toUserResp = await _queries.GetUserFromUserID(requestData.ToUserID);
            if (toUserResp.IsSuccessful == false)
                return new GenericResponseData { Success = false, Message = toUserResp.ErrorMessage };

            var fromUserResp = await _queries.GetUserFromUserID(requestData.FromUserID);
            if (fromUserResp.IsSuccessful == false)
                return new GenericResponseData { Success = false, Message = fromUserResp.ErrorMessage };

            User toUser = toUserResp.Data;
            User fromUser = fromUserResp.Data;

            if (toUser == null || fromUser == null)
                return new GenericResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} IsNull: {toUser == null} FromUser: {requestData.FromUserID} IsNull: {fromUser == null}" };

            try
            {
                string threadID = SharedStaticMethods.CreateHashedDirectMessageID(toUser.UserID, fromUser.UserID);
                var threadRemoveResponse = await _db.ChatThreadsContainer.DeleteItemAsync<ChatThread>(threadID, new PartitionKey(threadID));
                if (threadRemoveResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
                    return new GenericResponseData { Success = false, Message = $"Couldnt remove thread from database" };
            }
            catch (Exception ex)
            {
                return new GenericResponseData { Success = false, Message = $"RemoveFriend Remove Thread Exception: {ex.Message}" };
            }

            toUser.Friends.Remove(fromUser.UserID);
            toUser.FriendsVNum++;
            fromUser.Friends.Remove(toUser.UserID);
            fromUser.FriendsVNum++;

            try
            {
                var toUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(toUser, toUser.UserID, new PartitionKey(toUser.UserID));
                var fromUserReplaceResponse = await _db.UsersContainer.ReplaceItemAsync(fromUser, fromUser.UserID, new PartitionKey(fromUser.UserID));
                if (toUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK || fromUserReplaceResponse.StatusCode != System.Net.HttpStatusCode.OK)
                    return new GenericResponseData { Success = false, Message = $"Couldnt get users from database - ToUser: {requestData.ToUserID} Status: {toUserReplaceResponse.StatusCode} FromUser: {requestData.FromUserID} Status: {fromUserReplaceResponse.StatusCode}" };
            }
            catch (Exception ex)
            {
                return new GenericResponseData { Success = false, Message = $"RespondToFriendRequest Replace Exception: {ex.Message}" };
            }

            var deleteMessagesResponse = await _queries.DeleteMessagesByThreadID(requestData.ThreadID);
            if (deleteMessagesResponse.IsException)
                Console.WriteLine($"Remove friend from {requestData.FromUserID} to {requestData.ToUserID} could'nt delete messages");

            return new GenericResponseData { Success = true, Message = "Successfully removed friend" };
        }
    }
}
