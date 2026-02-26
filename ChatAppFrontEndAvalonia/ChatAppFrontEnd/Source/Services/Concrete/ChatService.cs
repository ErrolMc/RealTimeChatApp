using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Messages;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class ChatService : IChatService
    {
        private readonly ISignalRService _signalRService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICachingService _cachingService;
        private readonly INetworkCallerService _networkCaller;
        
        private HubConnection Connection => _signalRService.Connection;

        public IChatEntity CurrentChat { get; set; }
        public event Action<MessageCache> OnMessageReceived;

        public ChatService(ISignalRService signalRService, IAuthenticationService authenticationService, ICachingService cachingService, INetworkCallerService networkCaller)
        {
            _signalRService = signalRService;
            _authenticationService = authenticationService;
            _cachingService = cachingService;
            _networkCaller = networkCaller;
        }
        
        public async void OnReceiveMessage(Message message)
        {
            await _cachingService.CacheMessages(new List<Message>() { message });
            OnMessageReceived?.Invoke(message.ToMessageCache());
        }
        
        public async Task<bool> SendMessage(IChatEntity chatEntity, string messageContents)
        {
            string fromUserId = _authenticationService.CurrentUser.UserID;

            switch (chatEntity)
            {
                case UserSimple user:
                    {
                        var res = await SendMessage(SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, user.UserID), messageContents, user.UserID, MessageType.DirectMessage);
                        return res.success;
                    }
                case GroupDMSimple groupDM:
                    {
                        var res = await SendMessage(groupDM.GroupID, messageContents, string.Empty, MessageType.GroupMessage);
                        return res.success;
                    }
            }

            return false;
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
                    await _networkCaller.PerformBackendPostRequest<SendMessageRequestData, SendMessageResponseData>(EndpointNames.SEND_MESSAGE, requestData);

                if (response.ConnectionSuccess == false)
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
                
                await _cachingService.CacheMessages(new List<Message>() { responseData.Message });
                
                return (true, responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendDirectMessage Fail: {ex.Message}");
                return (false, null);
            }
        }
        
        public async Task<List<MessageCache>> GetMessages(IChatEntity chatEntity)
        {
            List<Message> uncachedMessages = await GetUncachedMessagesFromDatabase(chatEntity);
            if (uncachedMessages.Count > 0)
                await _cachingService.CacheMessages(uncachedMessages);
            List<MessageCache> allMessages = await _cachingService.GetMessagesFromThread(GetThreadIDFromChatEntity(chatEntity));
            return allMessages;
        }

        private async Task<List<Message>> GetUncachedMessagesFromDatabase(IChatEntity chatEntity)
        {
            string threadID = GetThreadIDFromChatEntity(chatEntity);
            long timeStamp = await _cachingService.GetThreadTimeStamp(threadID);
                    
            GetMessagesRequestData requestData = new GetMessagesRequestData()
            {
                ThreadID = threadID,
                LocalTimeStamp = timeStamp
            };
                    
            var response = 
                await _networkCaller.PerformBackendPostRequest<GetMessagesRequestData, GetMessagesResponseData>(EndpointNames.GET_MESSAGES, requestData);

            if (response.ConnectionSuccess)
                return response.ResponseData.Messages;
                    
            Console.WriteLine($"GetDirectMessages Fail: {response.Message}");
            return new List<Message>();
        }

        private string GetThreadIDFromChatEntity(IChatEntity chatEntity)
        {
            switch (chatEntity)
            {
                case UserSimple user:
                {
                    if (_authenticationService.CurrentUser == null)
                        return string.Empty;
                    
                    string fromUserId = _authenticationService.CurrentUser.UserID;
                    return SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, user.UserID);
                }
                case GroupDMSimple groupDM:
                {
                    return groupDM.GroupID;
                }
            }

            return string.Empty;
        }
    }
}

