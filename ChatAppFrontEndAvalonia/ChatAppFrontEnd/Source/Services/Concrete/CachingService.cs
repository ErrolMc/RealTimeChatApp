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
        private readonly ICacher _cacher;

        public CachingService()
        {
            _cacher = OperatingSystem.IsBrowser() ? new WebCacher() : new DesktopCacher();
        }

        public async Task<bool> Setup()
        {
            return await _cacher.Setup();
        }

        public async Task<bool> SaveLoginToken(string token)
        {
            return await _cacher.SaveLoginToken(token);
        }

        public async Task<(bool, string)> GetLoginToken()
        {
            return await _cacher.GetLoginToken();
        }

        public async Task<bool> ClearLoginToken()
        {
            return await _cacher.ClearLoginToken();
        }
        }
    }
}

