using System;
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
        private readonly ISignalRService _signalRService;
        private readonly IFriendService _friendService;

        private string _username;
        private string _password;
        private string _responseText;
        
        private bool _talkingToServer = false;

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

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        
        public LoginPanelViewModel(INavigationService navigationService, IAuthenticationService authenticationService, ISignalRService signalRService, IFriendService friendService, IOverlayService overlayService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _signalRService = signalRService;
            _friendService = friendService;

            ResponseText = string.Empty;
            TalkingToServer = false;
            
            LoginCommand = ReactiveCommand.Create(PerformLogin);
            GoToRegisterCommand = ReactiveCommand.Create(GoToRegister);

            if (DebugHelper.IS_DEBUG)
            {
                overlayService.ShowOverlay(new DebugLoginPanelViewModel((d_username, d_pass) =>
                {
                    Username = d_username;
                    Password = d_pass;
                    PerformLogin();
                    overlayService.HideOverlay();
                }), 100, 100, () => { });
            }
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

            var notificationResponse = await _signalRService.ConnectToSignalR(loginResponse.user);
            if (notificationResponse.success == false)
            {
                ResponseText = notificationResponse.message;
                TalkingToServer = false;
                return;
            }
            
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