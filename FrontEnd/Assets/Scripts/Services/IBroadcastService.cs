using System.Threading.Tasks;
using System;
using Zenject;

namespace ChatApp.Services
{
    public interface IBroadcastService
    {
        public Task BroadcastMessage(string message);
        public event Action<string> OnMessageReceived;
    }   
}
