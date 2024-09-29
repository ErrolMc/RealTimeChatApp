using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public partial class DesktopCacher
    {
        public async Task<List<MessageCache>> GetMessagesFromThread(string threadID)
        {
            try
            {
                var threads = await _messagesCollection.FindAsync(message => message.ThreadID == threadID);
                return threads.ToList();
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
                foreach (MessageCache message in messages)
                {
                    bool res = await _messagesCollection.UpsertAsync(message);
                }
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
                int res = await _messagesCollection.DeleteManyAsync(message => message.ThreadID == threadID);
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

