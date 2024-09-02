using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using ChatApp.Shared.TableDataSimple;
using Newtonsoft.Json; 

namespace ChatAppFrontEnd.Source.Other.Caching.Web
{
    public partial class WebCacher
    {
        public async Task<bool> SaveLoginToken(string token)
        {
            try
            {
                string message = await CallJavaScript.SaveLoginToken(token);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Save Token Error: " + e.Message);
                return false;
            }
        }
        
        public async Task<(bool, string)> GetLoginToken()
        {
            try
            {
                string token = await CallJavaScript.GetLoginToken();
                if (string.IsNullOrEmpty(token))
                    return (false, string.Empty);
                
                return (true, token);
            }
            catch (Exception e)
            {
                Console.WriteLine("Get Token Error: " + e.Message);
                return (false, string.Empty);
            }
        }

        public async Task<bool> ClearLoginToken()
        {
            try
            {
                string message = await CallJavaScript.SaveLoginToken(string.Empty);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}

