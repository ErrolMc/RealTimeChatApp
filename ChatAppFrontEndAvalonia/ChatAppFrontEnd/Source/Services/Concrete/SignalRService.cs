using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using ChatApp.Shared;
using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
                Console.WriteLine("SignalRService: Got token");
                
                try
                {
                    _connection = new HubConnectionBuilder()
                        .WithUrl($"{ServiceConfig.SignalRUri}/{Hub}", options =>
                        {
                            options.AccessTokenProvider = () => Task.FromResult(responseData.AccessToken);
                            
                        })
                        .AddMessagePackProtocol()
                        .Build();
                }
                catch (Exception e)
                {
                    Console.WriteLine("SignalRService Error: " + e.Message);
                    return (false, e.Message);
                }
                
                _connection.On("OnNotificationReceived", (string message) =>
                {
                    try
                    {
                        // need to convert from the signalR thread to the UI thread to avoid any potential issues
                        Dispatcher.UIThread.Post(() =>
                        {
                            bool res = _notificationService.HandleNotification(message); 
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"SignalRService Exception: {e.Message}");
                    }
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
            if (user == null)
            {
                return new AuthenticateResponseData() { Status = false, Message = "User is null" };
            }
            
            AuthenticateRequestData requestData = new AuthenticateRequestData()
            {
                UserName = user.Username, 
                UserID = user.UserID
            };
            
            var response = 
                await NetworkHelper.PerformBackendPostRequest<AuthenticateRequestData, AuthenticateResponseData>(EndpointNames.AUTHENTICATE_SIGNALR, requestData);
            
            if (response.ConnectionSuccess == false)
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

