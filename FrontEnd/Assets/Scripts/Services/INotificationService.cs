using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Services
{
    public interface INotificationService
    {
        public UniTask<(bool, string)> ConnectToSignalR(User user);
        public void OnApplicationQuit();
        public HubConnection Connection { get; }
    }
}