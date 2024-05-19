using System.Threading.Tasks;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppFrontEnd.Source.Services
{
    public interface ISignalRService
    {
        public Task<(bool success, string message)> ConnectToSignalR(User user);
        public HubConnection Connection { get; }
        public void OnApplicationQuit();
    }
}

