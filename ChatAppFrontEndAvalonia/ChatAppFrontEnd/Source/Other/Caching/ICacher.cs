using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;

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
    }
}
