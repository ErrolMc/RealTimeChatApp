using System.Threading.Tasks;
using LiteDB;
using LiteDB.Async;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public class TokenWrapper
    {
        [BsonId]
        public string ID { get; set; }
        public string Token { get; set; }
    }
    
    public partial class DesktopCacher
    {
        private const string LOGIN_TOKEN_ID = "LoginToken";
        
        private ILiteCollectionAsync<TokenWrapper> _tokenCollection;
        
        public async Task<bool> SaveLoginToken(string token)
        {
            TokenWrapper wrapper = new TokenWrapper()
            {
                ID = LOGIN_TOKEN_ID,
                Token = token
            };
            
            await _tokenCollection.UpsertAsync(wrapper); // upsert inserts if nothing is there, or updates if there is
            return true;
        }

        public async Task<(bool, string)> GetLoginToken()
        {
            var resp = await _tokenCollection.FindOneAsync(t => t.ID == LOGIN_TOKEN_ID);
            if (resp == null)
                return (false, string.Empty);
            return (true, resp.Token);
        }

        public async Task<bool> ClearLoginToken()
        {
            return await SaveLoginToken(string.Empty);
        }
    }
}