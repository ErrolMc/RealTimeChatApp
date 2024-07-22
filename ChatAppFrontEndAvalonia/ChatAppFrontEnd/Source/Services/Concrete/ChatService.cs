using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Messages;
using ChatApp.Shared.TableDataSimple;
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

        public IChatEntity CurrentChat { get; set; }
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
                    await NetworkHelper.PerformFunctionPostRequest<SendMessageRequestData, SendMessageResponseData>(FunctionNames.SEND_MESSAGE, requestData);

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

                return (true, responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendDirectMessage Fail: {ex.Message}");
                return (false, null);
            }
        }
        
        public async Task<List<Message>> GetMessages(IChatEntity chatEntity)
        {
            switch (chatEntity)
            {
                case UserSimple user:
                {
                    string fromUserId = _authenticationService.CurrentUser.UserID;
                    GetMessagesRequestData requestData = new GetMessagesRequestData()
                    {
                        ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, chatEntity.ID),
                    };
                    
                    var response = 
                        await NetworkHelper.PerformFunctionPostRequest<GetMessagesRequestData, GetMessagesResponseData>(FunctionNames.GET_MESSAGES, requestData);

                    if (!response.ConnectionSuccess)
                    {
                        Console.WriteLine($"GetDirectMessages Fail: {response.Message}");
                        return new List<Message>();
                    }
                    
                    return response.ResponseData.Messages;
                }
                case GroupDMSimple groupDM:
                {
                    GetMessagesRequestData requestData = new GetMessagesRequestData()
                    {
                        ThreadID = groupDM.GroupID
                    };

                    var response = 
                        await NetworkHelper.PerformFunctionPostRequest<GetMessagesRequestData, GetMessagesResponseData>(FunctionNames.GET_MESSAGES, requestData);

                    if (!response.ConnectionSuccess)
                    {
                        Console.WriteLine($"GetDirectMessages Fail: {response.Message}");
                        return new List<Message>();
                    }
            
                    return response.ResponseData.Messages;
                }
            }

            return new List<Message>();
        }
    }
}

