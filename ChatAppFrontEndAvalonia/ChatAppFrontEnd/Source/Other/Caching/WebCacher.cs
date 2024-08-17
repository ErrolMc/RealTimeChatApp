using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using ChatApp.Shared.TableDataSimple;
using Newtonsoft.Json; 

namespace ChatAppFrontEnd.Source.Other.Caching
{
    public partial class CallJavaScript
    {
        [JSImport("addUser", "app")]
        internal static partial Task<string> AddUser(string userJson);

        [JSImport("getAllUsers", "app")]
        internal static partial Task<string> GetAllUsers();
    }
    
    public class WebCacher : ICacher
    {
        public WebCacher()
        {
            
        }

        public async void Run()
        {
            await JSHost.ImportAsync("app", "../app.js"); 
            
            var friend1 = new UserSimple() { UserID = "id5", UserName = "name 5" }; 
            var friend2 = new UserSimple() { UserID = "id6", UserName = "name 6" }; 
            
            await AddFriend(friend1);
            await AddFriend(friend2);
            
            List<UserSimple> users = await GetFriends();
            foreach (var user in users)
            {
                Console.WriteLine($"{user.UserID}: {user.UserName}");
            }
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