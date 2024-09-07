using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Utils;
using ChatApp.Shared.Friends;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class FriendService : IFriendService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ICachingService _cachingService;
        
        public List<UserSimple> Friends { get; set; }
        public List<UserSimple> FriendRequests { get; set; }
        public List<UserSimple> OutgoingFriendRequests { get; set; }
        
        public event Action<UserSimple> OnFriendRequestReceived;
        public event Action<UserSimple> OnFriendRequestCanceled;
        public event Action<FriendRequestRespondNotification> OnFriendRequestRespondedTo;
        public event Action<UnfriendNotification> OnUnfriended;
        public event Action FriendsListUpdated;

        public FriendService(IAuthenticationService authenticationService, ICachingService cachingService)
        {
            _authenticationService = authenticationService;
            _cachingService = cachingService;
            
            Friends = new List<UserSimple>();
            FriendRequests = new List<UserSimple>();
            OutgoingFriendRequests = new List<UserSimple>();
        }
        
        public async Task<bool> UpdateFriendsList()
        {
            Friends.Clear();
            
            GetFriendsResponseData response = await GetFriends();
            if (response.Success)
            {
                Friends = response.Friends;
            }

            return response.Success;
        }

        public void AddFriendToLocalFriendsList(UserSimple user)
        {
            if (Friends.Contains(user))
                return;
            
            Friends.Add(user);
            FriendsListUpdated?.Invoke();
        }

        public async Task<bool> GetFriendRequests()
        {
            if (_authenticationService?.CurrentUser == null)
                return false;
            
            var response = 
                await NetworkHelper.PerformFunctionPostRequest<UserSimple, GetFriendRequestsResponseData>(FunctionNames.GET_FRIEND_REQUESTS, _authenticationService.CurrentUser.ToUserSimple());

            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine($"GetFriendRequests Error: {response.Message}");
                return false;
            }
            
            GetFriendRequestsResponseData responseData = response.ResponseData;
            if (responseData.Success == false)
            {
                Console.WriteLine($"GetFriendRequests Error: {responseData.Message}");
                return false;
            }
            
            FriendRequests = responseData.FriendRequests;
            OutgoingFriendRequests = responseData.OutgoingFriendRequests;
            return true;
        }
        
        #region Called from NotificationService
        /// <summary>
        /// When someone that sent this user a friend request cancels it
        /// </summary>
        /// <param name="user">The user that canceled their friend request</param>
        public void OnCancelFriendRequest(UserSimple user)
        {
            FriendRequests.Remove(user);
            OnFriendRequestCanceled?.Invoke(user);
        }

        /// <summary>
        /// When a friend request that this client sent gets responded to
        /// </summary>
        /// <param name="response">The response data</param>
        public void ProcessFriendRequestResponse(FriendRequestRespondNotification response)
        {
            if (response.Status)
            {
                AddFriendToLocalFriendsList(response.ToUser);
            }

            OutgoingFriendRequests.Remove(response.ToUser);
            OnFriendRequestRespondedTo?.Invoke(response);
        }
        
        /// <summary>
        /// When someone sends this user a friend request
        /// </summary>
        /// <param name="notification">The friend request data</param>
        public void OnReceiveFriendRequestNotification(FriendRequestNotification notification)
        {
            FriendRequests.Add(notification.FromUser);
            OnFriendRequestReceived?.Invoke(notification.FromUser);
        }
        
        public void OnUnfriend(UnfriendNotification notification)
        {
            UserSimple friend = Friends.FirstOrDefault(f => f.UserID == notification.FromUserID);
            if (friend == null)
                return;
            
            Friends.Remove(friend);
            OnUnfriended?.Invoke(notification);
            FriendsListUpdated?.Invoke();
        }
        #endregion
        
        #region Calling Functions
        private async Task<GetFriendsResponseData> GetFriends()
        {
            if (_authenticationService?.CurrentUser == null)
                return new GetFriendsResponseData() { Success = false, Message = "Current User is null"};

            int vNum = await _cachingService.GetFriendsVNum();
            
            GetFriendsRequestData requestData = new GetFriendsRequestData
            {   
                UserID = _authenticationService.CurrentUser.UserID,
                LocalVNum = vNum
            };
            
            var response = await NetworkHelper.PerformFunctionPostRequest<GetFriendsRequestData, GetFriendsResponseData>(FunctionNames.GET_FRIENDS, requestData);

            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine("FriendService - Failed to Get Friends, request failed");
                return new GetFriendsResponseData() { Success = false, Message = "Request failed" };
            }
            
            if (response.ResponseData.Success == false)
            {
                Console.WriteLine("FriendService - Failed to Get Friends");
                return new GetFriendsResponseData() { Success = false, Message = "Request failed" };
            }

            if (response.ResponseData.HasUpdate == false)
            {
                Console.WriteLine("FriendService - GetFriends: Getting from cache");
                response.ResponseData.Friends = await _cachingService.GetFriends();
                return response.ResponseData;
            }

            bool cacheSuccess = await _cachingService.CacheFriends(response.ResponseData.Friends, response.ResponseData.VNum);

            Console.WriteLine("FriendService - GetFriends: " + response.Message);
            return response.ResponseData;
        }
        
        public async Task<(bool success, string message, UserSimple user)> SendFriendRequest(string userName)
        {
            if (_authenticationService?.CurrentUser == null)
                return (false, "Current user is null", null);

            if (userName == _authenticationService.CurrentUser.Username)
                return (false, "Cant send a friend request to yourself!", null);
            
            var notificationData = new FriendRequestNotification()
            {   
                FromUser = _authenticationService.CurrentUser.ToUserSimple(),
                ToUserName = userName
            };
            
            var response =
                await NetworkHelper.PerformFunctionPostRequest<FriendRequestNotification, FriendRequestNotificationResponseData>(FunctionNames.SEND_FRIEND_REQUEST, notificationData);

            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine("FriendService: Request failed");
                return (false, "Request failed", null);
            }

            FriendRequestNotificationResponseData responseData = response.ResponseData;
            Console.WriteLine("FriendService - AddFriend: " + responseData.Message);
            
            return (responseData.Status, responseData.Message, responseData.ToUser);
        }

        public async Task<(bool success, string message)> RespondToFriendRequest(string fromUserID, string toUserID, bool status, bool isCanceling = false)
        {
            var notificationData = new RespondToFriendRequestData()
            {   
                FromUserID = fromUserID,
                ToUserID = toUserID,
                Status = status,
                isCanceling = isCanceling
            };
            
            var response =
                await NetworkHelper.PerformFunctionPostRequest<RespondToFriendRequestData, RespondToFriendRequestResponseData>(FunctionNames.RESPOND_TO_FRIEND_REQUEST, notificationData);

            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine("FriendService - Failed to respond to friend request");
                return (false, "Request failed");
            }
            
            RespondToFriendRequestResponseData responseData = response.ResponseData;
            Console.WriteLine("FriendService - Respond to request: " + responseData.Message);
            
            return (responseData.Success, responseData.Message);
        }
        
        public async Task<(bool success, string message)> UnFriend(string toUserID)
        {
            string fromUserID = _authenticationService.CurrentUser.UserID;
            var notificationData = new UnfriendNotification()
            {
                FromUserID = fromUserID,
                ToUserID = toUserID,
                ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(fromUserID, toUserID)
            };
            
            var response = await NetworkHelper.PerformFunctionPostRequest<UnfriendNotification, GenericResponseData>(FunctionNames.REMOVE_FRIEND, notificationData);
            
            if (response.ConnectionSuccess == false)
            {
                Console.WriteLine("FriendService - Unfriend failed");
                return (false, "Request failed");
            }
            
            GenericResponseData responseData = response.ResponseData;

            if (responseData.Success)
            {
                OnUnfriend(new UnfriendNotification() { FromUserID = notificationData.ToUserID, ToUserID = notificationData.FromUserID });
            }
            
            Console.WriteLine($"FriendService - Unfriend with {toUserID} result: {responseData.Message}");
            return (responseData.Success, responseData.Message);
        }
        #endregion
    }   
}