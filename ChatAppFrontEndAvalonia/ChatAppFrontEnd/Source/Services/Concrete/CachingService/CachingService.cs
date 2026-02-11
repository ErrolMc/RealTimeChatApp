using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using ChatAppFrontEnd.Source.Other.Caching.Desktop;
using ChatAppFrontEnd.Source.Other.Caching.Web;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public partial class CachingService : ICachingService
    {
        private const string LOGIN_TOKEN_KEY = "LoginToken";
        private const string IS_LOGGED_IN_KEY = "IsLoggedIn";
        private const string FRIENDS_VNUM_KEY = "FriendsVNum";
        private const string GROUPS_VNUM_KEY = "GroupsVNum";
        
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

        public async Task<bool> SaveIsLoggedIn(bool isLoggedIn)
        {
            return await _cacher.SaveString(IS_LOGGED_IN_KEY, isLoggedIn.ToString());
        }

        public void OnDisconnect()
        {
            _cacher.SaveStringSync(IS_LOGGED_IN_KEY, false.ToString());
        }

        public async Task<bool> GetIsLoggedIn()
        {
            (bool success, string value) = await _cacher.GetString(IS_LOGGED_IN_KEY);
            return success && bool.TryParse(value, out bool result) && result;
        }
        #endregion
        
        #region groupDMs
        public async Task<int> GetGroupDMsVNum()
        {
            return await GetIntVNum(GROUPS_VNUM_KEY);
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

        #region messages
        public async Task<List<MessageCache>> GetMessagesFromThread(string threadID)
        {
            return await _cacher.GetMessagesFromThread(threadID);
        }

        public async Task<bool> CacheMessages(List<Message> messages)
        {
            List<MessageCache> messageCaches = messages.Select(message => message.ToMessageCache()).ToList();
            bool timeStampResult = await UpdateThreadTimeStamp(messages);
            
            return await _cacher.CacheMessages(messageCaches);
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

