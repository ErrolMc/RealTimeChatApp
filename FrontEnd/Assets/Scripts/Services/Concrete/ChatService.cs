using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Zenject;
using ChatApp.Shared;
using ChatApp.Shared.Tables;
using ChatApp.Utils;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ChatApp.Shared.Messages;
using UnityEngine;

namespace ChatApp.Services.Concrete
{
    public class ChatService : IChatService
    {
        [Inject] private INotificationService _notificationService;

        private HubConnection Connection => _notificationService.Connection;

        public event Action<Message> OnMessageReceived;

        public void ProcessMessage(Message message)
        {
            OnMessageReceived?.Invoke(message);
        }

        public async UniTask<bool> SendDirectMessage(string fromUserId, string toUserID, string messageContents)
        {
            try
            {
                SaveMessageRequestData requestData = new SaveMessageRequestData()
                {
                    ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, toUserID),
                    FromUserID = fromUserId,
                    Message = messageContents,
                };
                
                (bool success, string message, SaveMessageResponseData responseData) = 
                    await NetworkHelper.PerformFunctionPostRequest<SaveMessageRequestData, SaveMessageResponseData>("SaveMessageToDB", requestData);

                if (success == false)
                {
                    // post request failed
                    
                    // retry send?
                    // put in a message queue
                    Debug.LogError($"SendDirectMessage Crash: {message}");
                    return false;
                }

                if (responseData.Success == false)
                {
                    // something failed in the function
                    Debug.LogError($"SendDirectMessage Fail: {responseData.ResponseMessage}");
                    return false;
                }
                
                await Connection.SendAsync("SendDirectMessage", toUserID, JsonConvert.SerializeObject(responseData.Message));

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async UniTask<List<Message>> GetDirectMessages(string userId1, string userId2)
        {
            GetMessagesRequestData requestData = new GetMessagesRequestData()
            {
                ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(userId1, userId2),
            };

            (bool success, string message, GetMessagesResponseData responseData) = 
                await NetworkHelper.PerformFunctionPostRequest<GetMessagesRequestData, GetMessagesResponseData>("GetMessages", requestData);

            if (!success)
            {
                Debug.LogError($"GetDirectMessages Fail: {responseData.ResponseMessage}");
            }
            
            return responseData.Messages;
        }
    }   
}
