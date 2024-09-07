using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using Newtonsoft.Json; 

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class WebCacher
    {
        public async Task<bool> CacheFriends(List<UserSimple> friends)
        {
            try
            {
                string json = JsonConvert.SerializeObject(friends);
                string result = await CallJavaScript.CacheFriends(json);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("CacheFriends Error: " + e.Message);
                return false;
            }
        }
        
        public async Task<List<UserSimple>> GetFriends()
        {
            try
            {
                string json = await CallJavaScript.GetFriends();
                List<UserSimple> friends = JsonConvert.DeserializeObject<List<UserSimple>>(json);
                return friends;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetFriends Error: " + e.Message);
                return new List<UserSimple>();
            }
        }
    }
}
