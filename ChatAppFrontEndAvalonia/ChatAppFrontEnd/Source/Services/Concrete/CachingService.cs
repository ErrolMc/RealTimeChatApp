using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching;
using ChatAppFrontEnd.Source.Other.Caching.Desktop;
using ChatAppFrontEnd.Source.Other.Caching.Web;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class CachingService : ICachingService
    {
        private const string LOGIN_TOKEN_KEY = "LoginToken";
        private const string FRIENDS_VNUM_KEY = "FriendsVNum";
        
        private readonly ICacher _cacher;

        public CachingService()
        {
            _cacher = OperatingSystem.IsBrowser() ? new WebCacher() : new DesktopCacher();
        }

        public async Task<bool> Setup()
        {
            return await _cacher.Setup();
        }
        
        public async Task<bool> ClearCache()
        {
            return await _cacher.ClearCache();
        }
        
        #region auth
        public async Task<bool> SaveLoginToken(string token)
        {
            return await _cacher.SaveString(LOGIN_TOKEN_KEY, token);
        }

        public async Task<(bool, string)> GetLoginToken()
        {
            return await _cacher.GetString(LOGIN_TOKEN_KEY);
        }
        #endregion

        #region messages
        public async Task<int> GetThreadVNum(string threadID)
        {
            await Task.Delay(1);
            return -1;
        }

        public async Task<List<MessageSimple>> GetMessagesFromThread(string threadID)
        {
            await Task.Delay(1);
            return new List<MessageSimple>();
        }

        public async Task<bool> AddMessagesToThread(string threadID, List<MessageSimple> messages)
        {
            await Task.Delay(1);
            return true;
        }

        public async Task<bool> ClearMessageThread(string threadID)
        {
            await Task.Delay(1);
            return true;
        }
        #endregion

        #region friends
        public async Task<int> GetFriendsVNum()
        {
            return await GetIntVNum(FRIENDS_VNUM_KEY);
        }
        
        public async Task<List<UserSimple>> GetFriends()
        {
            return await _cacher.GetFriends();
        }

        public async Task<bool> CacheFriends(List<UserSimple> friends, int vNum)
        {
            if (!await _cacher.CacheFriends(friends))
                return false;
            return await SaveIntVNum(FRIENDS_VNUM_KEY, vNum);
        }
        #endregion
        
        #region shared

        private async Task<bool> SaveIntVNum(string key, int vNum)
        {
            return await _cacher.SaveString(key, vNum.ToString());
        }
        
        private async Task<int> GetIntVNum(string key)
        {
            (bool success, string str) = await _cacher.GetString(key);
            
            if (!success)
                return -1;
            
            if (int.TryParse(str, out int value))
                return value;
            
            return -1;
        }
        #endregion
    }
}

