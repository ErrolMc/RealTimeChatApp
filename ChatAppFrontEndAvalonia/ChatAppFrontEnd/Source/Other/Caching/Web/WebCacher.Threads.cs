using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class WebCacher
    {
        public async Task<List<ThreadCache>> GetAllThreads()
        {
            try
            {
                string json = await CallJavaScript.GetAllThreads();
                List<ThreadCache> threads = JsonConvert.DeserializeObject<List<ThreadCache>>(json);
                return threads;
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
                string json = JsonConvert.SerializeObject(threadIDs);
                string result = await CallJavaScript.RemoveThreads(json);
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
                string json = JsonConvert.SerializeObject(threads);
                string result = await CallJavaScript.AddThreads(json);
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
                string json = JsonConvert.SerializeObject(thread);
                string result = await CallJavaScript.UpdateThread(json);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("SetThreadVNum Error: " + e.Message);
                return false;
            }
        }
    }
}
