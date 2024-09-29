using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using LiteDB;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public partial class DesktopCacher
    {
        public async Task<List<ThreadCache>> GetAllThreads()
        {
            try
            {
                var threads = await _threadsCollection.FindAllAsync();
                return threads.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetAllThreads Error: " + e.Message);
                return new List<ThreadCache>();
            }
        }

        public async Task<bool> RemoveThreads(List<string> threadIDs)
        {
            try
            {
                foreach (string threadID in threadIDs)
                {
                    if (!await _threadsCollection.ExistsAsync(td => td.ThreadID == threadID))
                        continue;
                    await _threadsCollection.DeleteAsync(new BsonValue(threadID));
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("RemoveThreads Error: " + e.Message);
                return false;
            }
        }

        public async Task<bool> AddThreads(List<ThreadCache> threads)
        {
            try
            {
                foreach (ThreadCache thread in threads)
                {
                    if (await _threadsCollection.ExistsAsync(td => td.ThreadID == thread.ThreadID))
                        continue;
                    await _threadsCollection.InsertAsync(thread);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("AddThreads Error: " + e.Message);
                return false;
            }
        }

        public async Task<bool> UpdateThread(ThreadCache thread)
        {
            try
            {
                await _threadsCollection.UpsertAsync(new BsonValue(thread.ThreadID), thread);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("UpdateThread Error: " + e.Message);
                return false;
            }
        }
    }
}

