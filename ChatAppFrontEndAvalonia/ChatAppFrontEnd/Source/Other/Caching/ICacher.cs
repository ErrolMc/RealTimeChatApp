using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Other.Caching
{
    public interface ICacher
    {
        Task<bool> Setup();
        Task<bool> ClearCache();
        
        Task<bool> SaveString(string key, string value);
        Task<(bool, string)> GetString(string key);
        
        Task<List<UserSimple>> GetFriends();
        Task<bool> CacheFriends(List<UserSimple> friends);

        Task<List<ThreadCache>> GetAllThreads();
        Task<bool> RemoveThreads(List<string> threadIDs);
        Task<bool> AddThreads(List<ThreadCache> threads);
        Task<bool> UpdateThread(ThreadCache thread);
        
        Task<List<MessageCache>> GetMessagesFromThread(string threadID);
        Task<bool> CacheMessages(List<MessageCache> messages);
        Task<bool> ClearMessageThread(string threadID);
    }
}
