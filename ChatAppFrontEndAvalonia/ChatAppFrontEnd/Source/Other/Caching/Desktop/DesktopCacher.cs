using System.Threading.Tasks;
using LiteDB.Async;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public partial class DesktopCacher : ICacher
    {
        private const string CONNECTION_STRING = @"Filename=ErrolChatCache.db; Connection=shared";
        
        private LiteDatabaseAsync _db;

        public DesktopCacher()
        {
            
        }

        public async Task<bool> Setup()
        {
            await Task.Delay(1);
            
            _db = new LiteDatabaseAsync(CONNECTION_STRING);
            _tokenCollection = _db.GetCollection<TokenWrapper>("tokens");
            return true;
        }
    }
}

