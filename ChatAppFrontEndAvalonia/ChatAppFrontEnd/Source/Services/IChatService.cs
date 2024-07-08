using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using ChatApp.Shared.Tables;

namespace ChatAppFrontEnd.Source.Services
{
    public interface IChatService
    {
        public event Action<Message> OnMessageReceived;

        public void OnReceiveMessage(Message message);
        public Task<bool> SendDirectMessage(string toUserID, string messageContents);
        public Task<bool> SendGroupDMMessage(string threadID, string messageContents);
        public Task<List<Message>> GetDirectMessages(string userId1, string userId2);
        public Task<List<Message>> GetMessages(string threadID);
    }   
}