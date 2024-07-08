using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Messages;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class ChatService : IChatService
    {
        private readonly ISignalRService _signalRService;
        private readonly IAuthenticationService _authenticationService;
        
        private HubConnection Connection => _signalRService.Connection;

        public event Action<Message> OnMessageReceived;

        public ChatService(ISignalRService signalRService, IAuthenticationService authenticationService)
        {
            _signalRService = signalRService;
            _authenticationService = authenticationService;
        }
        
        public void OnReceiveMessage(Message message)
        {
            OnMessageReceived?.Invoke(message);
        }

        public async Task<bool> SendDirectMessage(string toUserID, string messageContents)
        {
            string fromUserId = _authenticationService.CurrentUser.UserID;
            var res = await SendMessage(SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, toUserID), messageContents, toUserID, MessageType.DirectMessage);
            return res.success;
        }
        
        public async Task<bool> SendGroupDMMessage(string threadID, string messageContents)
        {
            var res = await SendMessage(threadID, messageContents, string.Empty, MessageType.GroupMessage);
            return res.success;
        }

        private async Task<(bool success, SendMessageResponseData response)> SendMessage(string threadID, string messageContents, string metaData, MessageType messageType)
        {
            try
            {
                string fromUserId = _authenticationService.CurrentUser.UserID;
                
                SendMessageRequestData requestData = new SendMessageRequestData()
                {
                    ThreadID = threadID,
                    FromUserID = fromUserId,
                    Message = messageContents,
                    MessageType = (int)messageType,
                    MetaData = metaData
                };
                
                var response = 
                    await NetworkHelper.PerformFunctionPostRequest<SendMessageRequestData, SendMessageResponseData>(FunctionNames.SEND_MESSAGE, requestData);

                if (response.Success == false)
                {
                    // post request failed
                    
                    // retry send?
                    // put in a message queue
                    Console.WriteLine($"SendDirectMessage Crash: {response.Message}");
                    return (false, null);
                }

                var responseData = response.ResponseData;
                if (responseData.Success == false)
                {
                    // something failed in the function
                    Console.WriteLine($"SendDirectMessage Fail: {responseData.ResponseMessage}");
                    return (false, null);
                }

                return (true, responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendDirectMessage Fail: {ex.Message}");
                return (false, null);
            }
        }

        public async Task<List<Message>> GetDirectMessages(string userId1, string userId2)
        {
            GetMessagesRequestData requestData = new GetMessagesRequestData()
            {
                ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(userId1, userId2),
            };

            var response = 
                await NetworkHelper.PerformFunctionPostRequest<GetMessagesRequestData, GetMessagesResponseData>(FunctionNames.GET_MESSAGES, requestData);

            if (!response.Success)
            {
                Console.WriteLine($"GetDirectMessages Fail: {response.Message}");
            }
            
            return response.ResponseData.Messages;
        }
        
        public async Task<List<Message>> GetMessages(string threadID)
        {
            GetMessagesRequestData requestData = new GetMessagesRequestData()
            {
                ThreadID = threadID
            };

            var response = 
                await NetworkHelper.PerformFunctionPostRequest<GetMessagesRequestData, GetMessagesResponseData>(FunctionNames.GET_MESSAGES, requestData);

            if (!response.Success)
            {
                Console.WriteLine($"GetDirectMessages Fail: {response.Message}");
            }
            
            return response.ResponseData.Messages;
        }
    }
}

