using System.Threading.Tasks;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppFrontEnd.Source.Services
{
    public interface INotificationService
    {
        public Task<(bool status, string message)> ConnectToSignalR(User user);
        public void OnApplicationQuit();
        public HubConnection Connection();
    }
}