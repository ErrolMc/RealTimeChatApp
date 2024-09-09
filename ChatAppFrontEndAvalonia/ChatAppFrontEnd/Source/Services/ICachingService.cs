using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppFrontEnd.Source.Services
{
    public interface ICachingService
    {
        Task<bool> Setup();
        
        // auth
        Task<bool> SaveLoginToken(string token);
        Task<(bool, string)> GetLoginToken();
        Task<bool> ClearCache();
        
        // messages
        Task<int> GetThreadVNum(string threadID);
        Task<List<MessageSimple>> GetMessagesFromThread(string threadID);
        Task<bool> AddMessagesToThread(string threadID, List<MessageSimple> messages);
        Task<bool> ClearMessageThread(string threadID);
        
        // friends
        Task<List<UserSimple>> GetFriends();
        Task<int> GetFriendsVNum();
        Task<bool> CacheFriends(List<UserSimple> friends, int vNum);
    }
}

