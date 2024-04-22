using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Constants;
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
        [Inject] private IFriendService _friendService;
        
        private const string Hub = "NotificationHub";
        private HubConnection _connection;
        private bool _connected = false;
        
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
        
        private async UniTask<AuthenticateResponseData> PerformTokenRequest(User user)
        {
            AuthenticateRequestData requestData = new AuthenticateRequestData()
            {
                UserName = user.Username, 
                UserID = user.UserID
            };

            string json = JsonConvert.SerializeObject(requestData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
            
            using UnityWebRequest request = UnityWebRequest.Put(NetworkConstants.FUNCTIONS_URI + "api/AuthenticateSignalR", jsonToSend);
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("Content-Type", "application/json");
            
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return new AuthenticateResponseData() { Status = false, Message = $"Request Failed: \n{request.error}", AccessToken = string.Empty };
            }
            
            AuthenticateResponseData responseData = JsonConvert.DeserializeObject<AuthenticateResponseData>(request.downloadHandler.text);
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
                    var friendRequestNotification = JsonConvert.DeserializeObject<FriendRequestNotification>(notificationData.NotificationJson);
                    
                    Debug.LogError("Friend request from " + friendRequestNotification.FromUserName);
                    _friendService.OnReceiveFriendRequestNotification(friendRequestNotification);
                    break;
            }

            return true;
        }
    }
}