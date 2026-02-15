using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatApp.Services;
using ChatAppFrontEnd.Source.Other;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Views;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class LoginPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICachingService _cachingService;
        private readonly ISignalRService _signalRService;
        private readonly IFriendService _friendService;
        private readonly IOverlayService _overlayService;

        private string _username;
        private string _password;
        private string _responseText;
        
        private bool _talkingToServer = false;
        private bool _isMainContentVisible = false;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }
        
        public string ResponseText
        {
            get => _responseText;
            set => this.RaiseAndSetIfChanged(ref _responseText, value);
        }
        
        public bool TalkingToServer
        {
            get => _talkingToServer;
            set => this.RaiseAndSetIfChanged(ref _talkingToServer, value);
        }
        
        public bool IsMainContentVisible
        {
            get => _isMainContentVisible;
            set => this.RaiseAndSetIfChanged(ref _isMainContentVisible, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        
        public LoginPanelViewModel(INavigationService navigationService, IAuthenticationService authenticationService, ISignalRService signalRService, IFriendService friendService, IOverlayService overlayService, ICachingService cachingService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _signalRService = signalRService;
            _friendService = friendService;
            _cachingService = cachingService;
            _overlayService = overlayService;

            ResponseText = string.Empty;
            TalkingToServer = false;
            IsMainContentVisible = false;
            
            LoginCommand = ReactiveCommand.Create(PerformLogin);
            GoToRegisterCommand = ReactiveCommand.Create(GoToRegister);

            if (DebugHelper.IS_DEBUG)
            {
                _overlayService.ShowOverlay(new DebugLoginPanelViewModel((d_username, d_pass) =>
                {
                    Username = d_username;
                    Password = d_pass;
                    PerformLogin();
                    _overlayService.HideOverlay();
                }), 100, 100, () => { });
            }
        }

        public override async void OnShow()
        {
            bool cacheSetup = await _cachingService.Setup();

            (bool getTokenSuccess, string token) = await _cachingService.GetLoginToken();
            if (getTokenSuccess == false)
            {
                IsMainContentVisible = true;
                return;
            }

            bool isAlreadyLoggedIn = await _cachingService.GetIsLoggedIn();
            if (isAlreadyLoggedIn)
            {
                await _cachingService.ClearCache();
                IsMainContentVisible = true;
                return;
            }

            var loginResponse = await _authenticationService.TryAutoLogin(token);
            if (loginResponse.success == false)
            {
                bool res = await _cachingService.ClearCache();
                IsMainContentVisible = true;
                return;
            }
            
            var notificationResponse = await _signalRService.ConnectToSignalR(loginResponse.user);
            if (notificationResponse.success == false)
            {
                IsMainContentVisible = true;
                return;
            }
            
            _authenticationService.CurrentUser = loginResponse.user;
            
            bool friendRequestResponse = await _friendService.GetFriendRequests();
            _overlayService.HideOverlay(); // hide debug overlay
            _navigationService.Navigate<MainPanelViewModel>();
        }

        private async void PerformLogin()
        {
            if (TalkingToServer) return;
            TalkingToServer = true;
            
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ResponseText = "Please fill in all fields!";
                TalkingToServer = false;
                return;
            }
            
            ResponseText = "Trying to login...";

            var loginResponse = await _authenticationService.TryLogin(Username, Password);
            if (loginResponse.success == false)
            {
                ResponseText = loginResponse.message;
                TalkingToServer = false;
                return;
            }

            ResponseText = "Login OK. Connecting to SignalR...";

            try
            {
                var notificationResponse = await _signalRService.ConnectToSignalR(loginResponse.user);
                if (notificationResponse.success == false)
                {
                    ResponseText = $"SignalR failed: {notificationResponse.message}";
                    TalkingToServer = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ResponseText = $"SignalR exception: {ex.Message}";
                TalkingToServer = false;
                return;
            }

            ResponseText = "SignalR connected. Loading data...";

            // setup data that other viewmodels will use
            _authenticationService.CurrentUser = loginResponse.user;
            bool friendRequestResponse = await _friendService.GetFriendRequests();

            // navigate to the main panel
            TalkingToServer = false;
            _navigationService.Navigate<MainPanelViewModel>();
        }

        private void GoToRegister()
        {
            _navigationService.Navigate<RegisterPanelViewModel>();
        }
    }
}