using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;

namespace ChatAppFrontEnd.Source.Services
{
    public interface IChatService
    {
        public event Action<Message> OnMessageReceived;

        public void OnReceiveMessage(Message message);
        public Task<bool> SendMessage(IChatEntity chatEntity, string messageContents);
        public Task<List<Message>> GetMessages(IChatEntity chatEntity);
    }   
}