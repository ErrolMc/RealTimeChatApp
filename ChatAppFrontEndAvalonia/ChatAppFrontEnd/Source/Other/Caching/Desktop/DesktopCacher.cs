using System;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using LiteDB;
using LiteDB.Async;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public class StringWrapper
    {
        [BsonId]
        public string Key { get; set; }
        public string Value { get; set; }
    }
    
    public partial class DesktopCacher : ICacher
    {
        private const string CONNECTION_STRING = @"Filename=ErrolChatCache.db; Connection=shared";
        private const string STRING_COLLECTION_NAME = "Strings";
        private const string FRIENDS_COLLECTION_NAME = "Friends";
        
        private LiteDatabaseAsync _db;
        private ILiteCollectionAsync<StringWrapper> _stringsCollection;
        private ILiteCollectionAsync<UserSimple> _friendsCollection;
        
        public async Task<bool> Setup()
        {
            await Task.Delay(1);

            try
            {
                _db = new LiteDatabaseAsync(CONNECTION_STRING);
                _stringsCollection = _db.GetCollection<StringWrapper>(STRING_COLLECTION_NAME);
                _friendsCollection = _db.GetCollection<UserSimple>(FRIENDS_COLLECTION_NAME);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cache Setup Error: " + e.Message);
                return false;
            }
        }

        public async Task<bool> ClearCache()
        {
            try
            {
                await _db.DropCollectionAsync(STRING_COLLECTION_NAME);
                await _db.DropCollectionAsync(FRIENDS_COLLECTION_NAME);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SaveString(string key, string value)
        {
            try
            {
                StringWrapper wrapper = new StringWrapper()
                {
                    Key = key,
                    Value = value
                };

                await _stringsCollection.UpsertAsync(wrapper); // upsert inserts if nothing is there, or updates if there is
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SaveString Error: " + e.Message);
                return false;
            }
        }

        public async Task<(bool, string)> GetString(string key)
        {
            try
            {
                var resp = await _stringsCollection.FindOneAsync(t => t.Key == key);
                if (resp == null)
                    return (false, string.Empty);
                return (true, resp.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("GetString Error: " + e.Message);
                return (false, string.Empty);
            }
        }
    }
}

