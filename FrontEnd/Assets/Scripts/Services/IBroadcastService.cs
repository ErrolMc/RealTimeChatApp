using System.Threading.Tasks;
using System;
using Cysharp.Threading.Tasks;
using Zenject;

namespace ChatApp.Services
{
    public interface IBroadcastService
    {
        public UniTask BroadcastMessage(string message);
        public event Action<string> OnMessageReceived;
    }   
}
