using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class WebCacher
    {
        public async Task<List<MessageCache>> GetMessagesFromThread(string threadID)
        {
            try
            {
                string json = await CallJavaScript.GetMessagesFromThread(threadID);
                List<MessageCache> messages = JsonConvert.DeserializeObject<List<MessageCache>>(json);
                return messages;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetMessagesFromThread Error: " + e.Message);
                return new List<MessageCache>();
            }
        }

        public async Task<bool> CacheMessages(List<MessageCache> messages)
        {
            try
            {
                string json = JsonConvert.SerializeObject(messages);
                string result = await CallJavaScript.CacheMessages(json);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("CacheMessages Error: " + e.Message);
                return false;
            }
        }

        public async Task<bool> ClearMessageThread(string threadID)
        {
            try
            {
                string result = await CallJavaScript.ClearMessageThread(threadID);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ClearMessageThread Error: " + e.Message);
                return false;
            }
        }
    }
}
