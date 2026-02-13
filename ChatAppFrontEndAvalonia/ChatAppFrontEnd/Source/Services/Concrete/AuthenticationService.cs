using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Utils;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICachingService _cachingService;
        
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }
     
        public AuthenticationService(ICachingService cachingService)
        {
            _cachingService = cachingService;
        }
        
        public async Task<(bool success, string message, User user)> TryLogin(string username, string password)
        {
            UserLoginData requestData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };
            
            var resp = await PerformLoginRequest(requestData);
            if (resp.ConnectionSuccess == false)
                return (false, resp.Message, null);
            return (resp.ResponseData.Status, resp.ResponseData.Message, resp.ResponseData.User);
        }

        public async Task<(bool success, string message, User user)> TryAutoLogin(string token)
        {
            AutoLoginData requestData = new AutoLoginData()
            {
                Token = token
            };
            
            var response = await NetworkHelper.PerformBackendPostRequest<AutoLoginData, UserLoginResponseData>(EndpointNames.AUTO_LOGIN, requestData);

            if (response.ConnectionSuccess == false)
                return (false, response.Message, null);

            UserLoginResponseData responseData = response.ResponseData;
            
            if (responseData.Status)
            {
                responseData.Status = false;

                bool tokenResult = await _cachingService.SaveLoginToken(responseData.LoginToken);
                if (tokenResult)
                {
                    await _cachingService.SaveIsLoggedIn(true);
                    responseData.Status = true;
                }
            }

            return (responseData.Status, responseData.Message, responseData.User);
        }

        public async Task<(bool success, string message)> TryRegister(string username, string password)
        {
            UserLoginData requestData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            var response = await NetworkHelper.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(EndpointNames.REGISTER, requestData);

            if (response.ConnectionSuccess == false)
                return (false, response.Message);
            return (response.ResponseData.Status, response.ResponseData.Message);
        }

        public async Task<bool> TryLogout()
        {
            return await _cachingService.ClearCache();
        }

        private static readonly HttpClient httpClient = new HttpClient();
        private async Task<BackendPostResponse<UserLoginResponseData>> PerformLoginRequest(UserLoginData requestData)
        {
            try
            {
                string json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PostAsync($"{ServiceConfig.BackendUri}/api/{EndpointNames.LOGIN}", content);

                if (!response.IsSuccessStatusCode)
                {
                    return new BackendPostResponse<UserLoginResponseData>() { ConnectionSuccess = false, Message = response.ReasonPhrase, ResponseData = null };
                }
                
                string responseContent = await response.Content.ReadAsStringAsync();
                UserLoginResponseData responseData = JsonConvert.DeserializeObject<UserLoginResponseData>(responseContent);

                // Retrieve the Set-Cookie header
                if (responseData.Status)
                {
                    responseData.Status = false;

                    bool tokenResult = await _cachingService.SaveLoginToken(responseData.LoginToken);
                    if (tokenResult)
                    {
                        await _cachingService.SaveIsLoggedIn(true);
                        responseData.Status = true;
                    }
                }

                return new BackendPostResponse<UserLoginResponseData>() { ConnectionSuccess = true, Message = "Request Success", ResponseData = responseData };
            }
            catch (Exception ex)
            {
                return new BackendPostResponse<UserLoginResponseData>() { ConnectionSuccess = false, Message = $"Exception: {ex.Message}", ResponseData = null };
            }
        }
    }
}