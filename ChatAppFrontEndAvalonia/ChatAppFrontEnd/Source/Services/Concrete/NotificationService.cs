using System;
using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class NotificationService : INotificationService
    {
        private readonly IFriendService _friendService;
        private readonly Lazy<IChatService> _chatService;
        
        private const string Hub = "NotificationHub";
        private HubConnection _connection;
        private bool _connected = false;

        public HubConnection Connection() => _connection;

        public NotificationService(IFriendService friendService, Lazy<IChatService> chatService)
        {
            _friendService = friendService;
            _chatService = chatService;
        }
        
        public async Task<(bool, string)> ConnectToSignalR(User user)
        {
            _connected = false;
            AuthenticateResponseData responseData = await PerformTokenRequest(user);

            if (responseData.Status)
            {
                Console.WriteLine("Got signalR token");
                
                try
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl(NetworkConstants.SIGNALR_URI + Hub, options =>
                        {
                            options.AccessTokenProvider = () => Task.FromResult(responseData.AccessToken);
                        })
                        .Build();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                    return (false, e.Message);
                }
                
                _connection.On("OnNotificationReceived", (string message) =>
                {
                    bool res = HandleNotification(message);
                });
                
                await _connection.StartAsync();
                
                Console.WriteLine("Successfully connected to SignalR");
                _connected = true;
                
                return (true, responseData.Message);
            }

            Console.WriteLine(responseData.Message);
            return (false, responseData.Message);
        }

        public void OnApplicationQuit()
        {
            if (_connected && _connection != null) 
                _connection.StopAsync();
        }

        private async Task<AuthenticateResponseData> PerformTokenRequest(User user)
        {
            AuthenticateRequestData requestData = new AuthenticateRequestData()
            {
                UserName = user.Username, 
                UserID = user.UserID
            };
            
            var response = 
                await NetworkHelper.PerformFunctionPostRequest<AuthenticateRequestData, AuthenticateResponseData>(FunctionNames.AUTHENTICATE_SIGNALR, requestData);
            
            if (response.Success == false)
            {
                return new AuthenticateResponseData() { Status = false, Message = $"PerformTokenRequest - Request Failed: \n{response.Message}", AccessToken = string.Empty };
            }
            
            return response.ResponseData;
        }
        
        private bool HandleNotification(string json)
        {
            Console.WriteLine("HandleNotification: " + json);
            
            var notificationData = JsonConvert.DeserializeObject<NotificationData>(json);
            if (notificationData == null)
                return false;

            NotificationType notificationType = (NotificationType)notificationData.NotificationType;
            switch (notificationType)
            {
                case NotificationType.FriendRequest:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestNotification>(notificationData.NotificationJson);
                        if (notification == null)
                            return false;
                        Console.WriteLine("Friend request from " + notification.FromUser.UserName);
                        _friendService.OnReceiveFriendRequestNotification(notification);   
                    }
                    break;
                case NotificationType.FriendRequestRespond:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestRespondNotification>(notificationData.NotificationJson);
                        Console.WriteLine($"Friend request responded from {notification.ToUser.UserName}: {notification.Status}");
                        _friendService.ProcessFriendRequestResponse(notification);
                    }
                    break;
                case NotificationType.FriendRequestCancel:
                    {
                        var fromUser = JsonConvert.DeserializeObject<UserSimple>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Friend request canceled from: {fromUser.UserName}");
                        _friendService.OnCancelFriendRequest(fromUser);
                    }
                    break;
                case NotificationType.DirectMessage:
                    {
                        var message = JsonConvert.DeserializeObject<Message>(notificationData.NotificationJson);
                        
                        Console.WriteLine($"Message from {message.FromUser.UserName}");
                        if (_chatService.IsValueCreated)
                            _chatService.Value.OnReceiveMessage(message);
                    }
                    break;
            }

            return true;
        }
    }
}