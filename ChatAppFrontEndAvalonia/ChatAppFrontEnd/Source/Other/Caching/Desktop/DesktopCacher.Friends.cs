using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontEnd.Source.Other.Caching.Desktop
{
    public partial class DesktopCacher
    {
        public async Task<List<UserSimple>> GetFriends()
        {
            try
            {
                var friends = await _friendsCollection.FindAllAsync();
                return friends.ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("GetFriends Error: " + e.Message);
                return new List<UserSimple>();
            }
        }

        public async Task<bool> CacheFriends(List<UserSimple> friends)
        {
            try
            {
                await _friendsCollection.DeleteAllAsync();
                await _friendsCollection.InsertAsync(friends);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("CacheFriends Error: " + e.Message);
                return false;
            }
        }
    }
}
