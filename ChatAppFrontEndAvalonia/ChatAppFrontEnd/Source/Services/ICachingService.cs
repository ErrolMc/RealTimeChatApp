using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Services
{
    public interface ICachingService
    {
        Task<bool> Setup();
        
        // auth
        Task<bool> SaveLoginToken(string token);
        Task<(bool, string)> GetLoginToken();
        Task<bool> ClearCache();
        
        // threads
        Task<List<ThreadCache>> GetAllThreads();
        Task<bool> RemoveThreads(List<string> threadIDs, bool removeMessages = true);
        Task<bool> AddThreads(List<ThreadCache> threadIDs);
        Task<long> GetThreadTimeStamp(string threadID);

        // messages
        Task<List<MessageCache>> GetMessagesFromThread(string threadID);
        Task<bool> CacheMessages(List<Message> messages);
        
        // friends
        Task<List<UserSimple>> GetFriends();
        Task<int> GetFriendsVNum();
        Task<bool> CacheFriends(List<UserSimple> friends, int vNum);
    }
}

