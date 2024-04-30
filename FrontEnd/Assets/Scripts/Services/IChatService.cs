using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;
using Zenject;

namespace ChatApp.Services
{
    public interface IChatService
    {
        public event Action<Message> OnMessageReceived;

        public void ProcessMessage(Message message);
        public UniTask<bool> SendDirectMessage(string fromUserId, string toUserID, string messageContents);
        public UniTask<List<Message>> GetDirectMessages(string userId1, string userId2);
    }   
}
