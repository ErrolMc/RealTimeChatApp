using ChatApp.Shared.Notifications;
using ChatApp.Shared.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = ChatApp.Shared.Tables.User;

namespace ChatAppDatabaseFunctions.Code
{
    public static class SharedQueries
    {
        public static async Task<User> GetUserFromUsername(string username)
        {
            IQueryable<User> query = DatabaseStatics.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.Username == username);
            FeedIterator<User> iterator = query.ToFeedIterator();
            FeedResponse<User> users = await iterator.ReadNextAsync();

            if (!users.Any())
                return null;

            return users.First();
        }

        public static async Task<User> GetUserFromUserID(string userID)
        {
            IQueryable<User> query = DatabaseStatics.UsersContainer.GetItemLinqQueryable<User>().Where(u => u.UserID == userID);
            FeedIterator<User> iterator = query.ToFeedIterator();
            FeedResponse<User> users = await iterator.ReadNextAsync();

            if (!users.Any())
                return null;

            return users.First();
        }
    }
}
