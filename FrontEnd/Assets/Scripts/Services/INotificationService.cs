using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;

namespace ChatApp.Services
{
    public interface INotificationService
    {
        public UniTask<(bool, string)> ConnectToSignalR(User user);
        public void OnApplicationQuit();
    }
}