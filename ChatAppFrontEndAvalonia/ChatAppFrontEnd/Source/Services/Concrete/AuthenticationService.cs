using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatApp.Shared;
using ChatApp.Shared.Authentication;
using ChatApp.Shared.Tables;
using Newtonsoft.Json;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICachingService _cachingService;
        private readonly INetworkCallerService _networkCaller;
        
        public bool IsLoggedIn { get; set; }
        public User CurrentUser { get; set; }
        public event Action OnLogout;

        public AuthenticationService(ICachingService cachingService, INetworkCallerService networkCaller)
        {
            _cachingService = cachingService;
            _networkCaller = networkCaller;
        }
        
        public async Task<(bool success, string message, User user)> TryLogin(string username, string password)
        {
            UserLoginData requestData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            return await PerformLoginRequest<UserLoginData>(requestData, EndpointNames.LOGIN);
        }

        public async Task<(bool success, string message, User user)> TryAutoLogin(string token)
        {
            AutoLoginData requestData = new AutoLoginData()
            {
                Token = token
            };
            
            return await PerformLoginRequest<AutoLoginData>(requestData, EndpointNames.AUTO_LOGIN);
        }

        public async Task<(bool success, string message)> TryRegister(string username, string password)
        {
            UserLoginData requestData = new UserLoginData()
            {
                UserName = username,
                Password = password
            };

            var response = await _networkCaller.PerformBackendPostRequest<UserLoginData, UserLoginResponseData>(EndpointNames.REGISTER, requestData);

            if (response.ConnectionSuccess == false)
                return (false, response.Message);
            return (response.ResponseData.Status, response.ResponseData.Message);
        }

        public async Task<bool> TryLogout()
        {
            bool result = await _cachingService.ClearCache();
            OnLogout?.Invoke();
            return result;
        }

        private async Task<(bool success, string message, User user)> PerformLoginRequest<T>(T requestData, string endpoint) where T : class
        {
            var response = await _networkCaller.PerformBackendPostRequest<T, UserLoginResponseData>(endpoint, requestData);

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
    }
}
