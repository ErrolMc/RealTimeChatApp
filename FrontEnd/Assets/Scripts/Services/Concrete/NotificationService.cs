using System;
using System.Threading.Tasks;
using ChatApp.Shared;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using ChatApp.Utils;
using MessagePipe;
using Microsoft.AspNetCore.SignalR.Client;
using Zenject;
using UnityEngine;
using UnityEngine.Networking;

namespace ChatApp.Services.Concrete
{
    public class NotificationService : INotificationService
    {
        [Inject] private IChatService _chatService;
        [Inject] private IFriendService _friendService;
        
        private const string Hub = "NotificationHub";
        private HubConnection _connection;
        private bool _connected = false;
        
        public HubConnection Connection => _connection;
        
        public async UniTask<(bool, string)> ConnectToSignalR(User user)
        {
            _connected = false;
            AuthenticateResponseData responseData = await PerformTokenRequest(user);

            if (responseData.Status)
            {
                Debug.LogError("Got signalR token");
                
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
                    Debug.LogError("Error: " + e.Message);
                    return (false, e.Message);
                }
                
                _connection.On("OnNotificationReceived", (string message) =>
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        bool res = HandleNotification(message);
                    });
                });
                
                await _connection.StartAsync();
                
                Debug.LogError("Successfully connected to SignalR");
                _connected = true;
                
                return (true, responseData.Message);
            }

            Debug.LogError(responseData.Message);
            return (false, responseData.Message);
        }

        public void OnApplicationQuit()
        {
            if (_connected && _connection != null) 
                _connection.StopAsync();
        }

        private async UniTask<AuthenticateResponseData> PerformTokenRequest(User user)
        {
            AuthenticateRequestData requestData = new AuthenticateRequestData()
            {
                UserName = user.Username, 
                UserID = user.UserID
            };
            
            (bool success, string message, AuthenticateResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<AuthenticateRequestData, AuthenticateResponseData>(FunctionNames.AUTHENTICATE_SIGNALR, requestData);
            
            if (success == false)
            {
                return new AuthenticateResponseData() { Status = false, Message = $"PerformTokenRequest - Request Failed: \n{message}", AccessToken = string.Empty };
            }
            
            return responseData;
        }
        
        private bool HandleNotification(string json)
        {
            Debug.LogError("HandleNotification: " + json);
            
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
                        Debug.LogError("Friend request from " + notification.FromUser.UserName);
                        _friendService.OnReceiveFriendRequestNotification(notification);   
                    }
                    break;
                case NotificationType.FriendRequestRespond:
                    {
                        var notification = JsonConvert.DeserializeObject<FriendRequestRespondNotification>(notificationData.NotificationJson);
                        Debug.LogError($"Friend request responded from {notification.ToUser.UserName}: {notification.Status}");
                        _friendService.ProcessFriendRequestResponse(notification);
                    }
                    break;
                case NotificationType.FriendRequestCancel:
                    {
                        var fromUser = JsonConvert.DeserializeObject<UserSimple>(notificationData.NotificationJson);
                        
                        Debug.LogError($"Friend request canceled from: {fromUser.UserName}");
                        _friendService.CancelFriendRequest(fromUser);
                    }
                    break;
                case NotificationType.DirectMessage:
                    {
                        var message = JsonConvert.DeserializeObject<Message>(notificationData.NotificationJson);
                        
                        Debug.LogError($"Message from {message.FromUser.UserName}");
                        _chatService.ProcessMessage(message);
                    }
                    break;
            }

            return true;
        }
    }
}