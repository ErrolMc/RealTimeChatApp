using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public partial class CachingService
    {
        private bool _hasGottenThreads = false;
        private Dictionary<string, ThreadCache> _threadDict;
        
        public async Task<List<ThreadCache>> GetAllThreads()
        {
            List<ThreadCache> threads = await _cacher.GetAllThreads();
            
            if (_hasGottenThreads)
                return threads;
            
            _threadDict = new Dictionary<string, ThreadCache>();
            threads.ForEach(thread => _threadDict.Add(thread.ThreadID, thread));
            
            _hasGottenThreads = true;
            return threads;
        }

        private async Task<bool> UpdateThreadTimeStamp(List<Message> messages)
        {
            if (messages.Count == 0)
                return false;
            
            string threadID = messages.First().ThreadID;
            long newMaxTimeStamp = messages.Max(msg => msg.TimeStamp);
            long cachedTimeStamp = await GetThreadTimeStamp(threadID);

            if (newMaxTimeStamp < cachedTimeStamp)
                return true;

            return await _cacher.UpdateThread(new ThreadCache() { ThreadID = threadID, Type = messages.First().MessageType, TimeStamp = newMaxTimeStamp });
        }
        
        public async Task<long> GetThreadTimeStamp(string threadID)
        {
            if (!_hasGottenThreads)
                await GetAllThreads();

            if (_threadDict.TryGetValue(threadID, out ThreadCache value))
                return value.TimeStamp;
            
            return -1;
        }

        public async Task<bool> RemoveThreads(List<string> threadIDs, bool removeMessages)
        {
            if (_hasGottenThreads)
                threadIDs.ForEach(threadID => _threadDict.Remove(threadID));
            
            bool removeThreadsSuccess = await _cacher.RemoveThreads(threadIDs);
            if (!removeThreadsSuccess || !removeMessages)
                return removeThreadsSuccess;
            
            // remove messages
            bool clearMessagesSuccess = true;
            foreach (string threadID in threadIDs)
            {
                bool res = await _cacher.ClearMessageThread(threadID);
                if (!res)
                    clearMessagesSuccess = false;
            }

            return clearMessagesSuccess;
        }

        public async Task<bool> AddThreads(List<ThreadCache> threads)
        {
            if (_hasGottenThreads)
                threads.ForEach(thread => _threadDict.Add(thread.ThreadID, thread));
            
            return await _cacher.AddThreads(threads);
        }
    }    
}
