using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Services
{
    public interface IChatService
    {
        public IChatEntity CurrentChat { get; set; }
        public event Action<MessageCache> OnMessageReceived;

        public void OnReceiveMessage(Message message);
        public Task<bool> SendMessage(IChatEntity chatEntity, string messageContents);
        public Task<List<MessageCache>> GetMessages(IChatEntity chatEntity);
    }   
}