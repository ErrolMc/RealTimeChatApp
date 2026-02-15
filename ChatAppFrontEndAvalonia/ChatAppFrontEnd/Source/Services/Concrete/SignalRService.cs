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

            Console.WriteLine($"[SignalR] Starting ConnectToSignalR for user {user?.Username}");
            Console.WriteLine($"[SignalR] SignalRUri = {ServiceConfig.SignalRUri}");
            Console.WriteLine($"[SignalR] BackendUri = {ServiceConfig.BackendUri}");

            AuthenticateResponseData responseData;
            try
            {
                responseData = await PerformTokenRequest(user);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SignalR] Token request exception: {e}");
                return (false, $"Token request failed: {e.Message}");
            }

            if (!responseData.Status)
            {
                Console.WriteLine($"[SignalR] Token request failed: {responseData.Message}");
                return (false, responseData.Message);
            }

            Console.WriteLine($"[SignalR] Got token (length={responseData.AccessToken?.Length ?? 0})");

            var hubUrl = $"{ServiceConfig.SignalRUri}/{Hub}";
            Console.WriteLine($"[SignalR] Connecting to hub: {hubUrl}");

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(responseData.AccessToken);
                    })
                    .AddMessagePackProtocol()
                    .Build();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SignalR] Build error: {e}");
                return (false, $"Hub build error: {e.Message}");
            }

            _connection.On("OnNotificationReceived", (string message) =>
            {
                try
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        bool res = _notificationService.HandleNotification(message);
                    });
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[SignalR] Notification exception: {e.Message}");
                }
            });

            try
            {
                Console.WriteLine("[SignalR] Calling StartAsync...");
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15));
                await _connection.StartAsync(cts.Token);
                Console.WriteLine("[SignalR] StartAsync completed successfully");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[SignalR] StartAsync timed out after 15 seconds");
                return (false, $"SignalR connection timed out (15s). Hub URL: {hubUrl}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SignalR] StartAsync error: {e}");
                return (false, $"SignalR connect failed: {e.Message}");
            }

            _connected = true;
            Console.WriteLine("[SignalR] Connected successfully");
            return (true, responseData.Message);
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

