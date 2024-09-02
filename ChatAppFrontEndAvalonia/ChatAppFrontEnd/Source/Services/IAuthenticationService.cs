using ChatApp.Shared.Tables;
using System.Threading.Tasks;

namespace ChatAppFrontEnd.Source.Services
{
    public interface IAuthenticationService
    {
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }
        public Task<(bool success, string message, User user)> TryLogin(string username, string password);
        public Task<(bool success, string message, User user)> TryAutoLogin(string token);
        public Task<(bool success, string message)> TryRegister(string username, string password);
        public Task<bool> TryLogout();
    }   
}
