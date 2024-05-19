using System.Threading.Tasks;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatAppFrontEnd.Source.Services
{
    public interface INotificationService
    {
        public bool HandleNotification(string json);
    }
}