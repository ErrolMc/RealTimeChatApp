using System;
using ChatAppFrontEnd.Source.Other.Caching;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class CachingService : ICachingService
    {
        private readonly ICacher _cacher;

        public CachingService()
        {
            _cacher = OperatingSystem.IsBrowser() ? new WebCacher() : new DesktopCacher();
        }

        public void Run()
        {
            _cacher.Run();
        }
    }
}

