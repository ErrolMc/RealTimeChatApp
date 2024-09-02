using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using ChatApp.Shared.TableDataSimple;
using Newtonsoft.Json; 

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class CallJavaScript
    {
        [JSImport("addUser", "app")]
        internal static partial Task<string> AddUser(string userJson);

        [JSImport("getAllUsers", "app")]
        internal static partial Task<string> GetAllUsers();
        
        [JSImport("saveLoginToken", "app")]
        internal static partial Task<string> SaveLoginToken(string token);

        [JSImport("getLoginToken", "app")]
        internal static partial Task<string> GetLoginToken();
    }
    
    public partial class WebCacher : ICacher
    {
        public WebCacher()
        {
            
        }

        public async Task<bool> Setup()
        {
            await JSHost.ImportAsync("app", "../app.js");

            return true;
        }

        private async Task<bool> AddFriend(UserSimple friend)
        {
            try
            {
                string json = JsonConvert.SerializeObject(friend);
                string result = await CallJavaScript.AddUser(json);
                return true;
            }
            catch (JSException ex) // This catches JavaScript exceptions specifically
            {
                Console.WriteLine("JavaScript error: " + ex.Message);
                return false;
            }
            catch (Exception ex) // Generic exception catch, in case other kinds of errors occur
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }

        private async Task<List<UserSimple>> GetFriends()
        {
            try
            {
                string usersJson = await CallJavaScript.GetAllUsers();
                List<UserSimple> users = JsonConvert.DeserializeObject<List<UserSimple>>(usersJson);
                return users;
            }
            catch (JSException ex) // This catches JavaScript exceptions specifically
            {
                Console.WriteLine("JavaScript error: " + ex.Message);
                return new List<UserSimple>();
            }
            catch (Exception ex) // Generic exception catch, in case other kinds of errors occur
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return new List<UserSimple>();
            }
        }
    }
}