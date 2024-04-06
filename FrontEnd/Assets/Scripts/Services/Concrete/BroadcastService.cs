using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using UnityEngine;
using Zenject;
using ChatApp.Utils;
using Cysharp.Threading.Tasks;

namespace ChatApp.Services.Concrete
{
    public class BroadcastService : IBroadcastService, IInitializable
    {
        private const string ServerAddress = "https://localhost:7003";
        private const string Hub = "/NotificationHub";
        
        private HubConnection _connection;
        public event Action<string> OnMessageReceived;

        public void Initialize()
        {
            Debug.LogWarning("BroadcastService: Initialize");

            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(ServerAddress + Hub)
                    .Build();
            }
            catch (Exception e)
            {
                Debug.LogError("Error: " + e.Message);
                return;
            }
            
            _connection.On("OnMessageReceived", (string message) =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    OnMessageReceived?.Invoke(message); 
                });
            });

            _connection.StartAsync();
        }
        
        public async UniTask BroadcastMessage(string message)
        {
            await _connection.SendAsync("BroadcastMessage", message);
        }
    }   
}
