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
            try
            {
                string fromUserId = _authenticationService.CurrentUser.UserID;
                
                SaveMessageRequestData requestData = new SaveMessageRequestData()
                {
                    ThreadID = SharedStaticMethods.CreateHashedDirectMessageID(fromUserId, toUserID),
                    FromUserID = fromUserId,
                    Message = messageContents,
                };
                
                var response = 
                    await NetworkHelper.PerformFunctionPostRequest<SaveMessageRequestData, SaveMessageResponseData>(FunctionNames.SAVE_MESSAGE_TO_DB, requestData);

                if (response.Success == false)
                {
                    // post request failed
                    
                    // retry send?
                    // put in a message queue
                    Console.WriteLine($"SendDirectMessage Crash: {response.Message}");
                    return false;
                }

                var responseData = response.ResponseData;
                if (responseData.Success == false)
                {
                    // something failed in the function
                    Console.WriteLine($"SendDirectMessage Fail: {responseData.ResponseMessage}");
                    return false;
                }
                
                await Connection.SendAsync("SendDirectMessage", toUserID, JsonConvert.SerializeObject(responseData.Message));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SendDirectMessage Fail: {ex.Message}");
                return false;
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
    }
}

