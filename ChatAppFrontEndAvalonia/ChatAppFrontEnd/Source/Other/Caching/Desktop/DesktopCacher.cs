using System;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using LiteDB;
using LiteDB.Async;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public partial class DesktopCacher : ICacher
    {
        private const string CONNECTION_STRING = @"Filename=ErrolChatCache.db; Connection=shared";
        private const string STRING_COLLECTION_NAME = "Strings";
        private const string FRIENDS_COLLECTION_NAME = "Friends";
        private const string THREADS_COLLECTION_NAME = "Threads";
        private const string MESSAGES_COLLECTION_NAME = "Messages";

        private LiteDatabaseAsync _db;
        private ILiteCollectionAsync<StringWrapper> _stringsCollection;
        private ILiteCollectionAsync<UserSimple> _friendsCollection;        
        private ILiteCollectionAsync<ThreadCache> _threadsCollection;
        private ILiteCollectionAsync<MessageCache> _messagesCollection;
        
        public async Task<bool> Setup()
        {
            await Task.Delay(1);

            try
            {
                _db = new LiteDatabaseAsync(CONNECTION_STRING);
                _stringsCollection = _db.GetCollection<StringWrapper>(STRING_COLLECTION_NAME);
                _friendsCollection = _db.GetCollection<UserSimple>(FRIENDS_COLLECTION_NAME);
                _threadsCollection = _db.GetCollection<ThreadCache>(THREADS_COLLECTION_NAME);
                _messagesCollection = _db.GetCollection<MessageCache>(MESSAGES_COLLECTION_NAME);
                
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
                await _db.DropCollectionAsync(THREADS_COLLECTION_NAME);
                await _db.DropCollectionAsync(MESSAGES_COLLECTION_NAME);
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

        public bool SaveStringSync(string key, string value)
        {
            try
            {
                using var db = new LiteDatabase(CONNECTION_STRING);
                var collection = db.GetCollection<StringWrapper>(STRING_COLLECTION_NAME);

                StringWrapper wrapper = new StringWrapper()
                {
                    Key = key,
                    Value = value
                };

                collection.Upsert(wrapper);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SaveStringSync Error: " + e.Message);
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

