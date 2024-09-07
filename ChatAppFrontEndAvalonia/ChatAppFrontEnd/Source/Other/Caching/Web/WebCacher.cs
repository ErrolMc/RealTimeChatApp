using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class CallJavaScript
    {
        [JSImport("SaveString", "functions")]
        internal static partial Task<string> SaveString(string key, string value);

        [JSImport("GetString", "functions")]
        internal static partial Task<string> GetString(string key);
        
        [JSImport("GetFriends", "functions")]
        internal static partial Task<string> GetFriends();

        [JSImport("CacheFriends", "functions")]
        internal static partial Task<string> CacheFriends(string friendsJson);
        
        [JSImport("ClearCache", "functions")]
        internal static partial Task<string> ClearCache();
    }
    
    public partial class WebCacher : ICacher
    {
        public async Task<bool> Setup()
        {
            try
            {
                await JSHost.ImportAsync("functions", "../functions.js");
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
                string message = await CallJavaScript.ClearCache();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ClearCache Error: " + e.Message);
                return false;
            }
        }

        public async Task<bool> SaveString(string key, string value)
        {
            try
            {
                string message = await CallJavaScript.SaveString(key, value);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Save String Error: " + e.Message);
                return false;
            }
        }

        public async Task<(bool, string)> GetString(string key)
        {
            try
            {
                string token = await CallJavaScript.GetString(key);
                if (string.IsNullOrEmpty(token))
                    return (false, string.Empty);
                
                return (true, token);
            }
            catch (Exception e)
            {
                Console.WriteLine("Get String Error: " + e.Message);
                return (false, string.Empty);
            }
        }
    }
}