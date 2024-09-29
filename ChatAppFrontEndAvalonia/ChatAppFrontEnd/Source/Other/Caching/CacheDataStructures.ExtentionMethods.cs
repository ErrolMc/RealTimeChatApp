using System.Collections.Generic;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;

namespace ChatAppFrontEnd.Source.Other.Caching.Data
{
    public static class CacheExtensionMethods
    {
        public static MessageCache ToMessageCache(this Message message)
        {
            return new MessageCache() { MessageID = message.ID, ThreadID = message.ThreadID, FromUser = message.FromUser, Message = message.MessageContents, TimeStamp = message.TimeStamp };
        }
    }
}

