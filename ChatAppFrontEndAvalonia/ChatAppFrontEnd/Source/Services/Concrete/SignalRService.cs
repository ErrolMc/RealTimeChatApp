using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Constants;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class SignalRService : ISignalRService
    {
        private readonly INotificationService _notificationService;
        
        private const string Hub = "NotificationHub";
        private HubConnection _connection;
        private bool _connected = false;

        public HubConnection Connection => _connection;
        
        public SignalRService(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<(bool success, string message)> ConnectToSignalR(User user)
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
                    bool res = _notificationService.HandleNotification(message);
                });
                
                await _connection.StartAsync();
                
                Console.WriteLine("Successfully connected to SignalR");
                _connected = true;
                
                return (true, responseData.Message);
            }

            Console.WriteLine(responseData.Message);
            return (false, responseData.Message);
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
        
        public void OnApplicationQuit()
        {
            if (_connected && _connection != null) 
                _connection.StopAsync();
        }
    }
}

