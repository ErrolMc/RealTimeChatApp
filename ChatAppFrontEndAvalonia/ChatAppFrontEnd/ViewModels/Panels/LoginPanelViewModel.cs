using System;
using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Views;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class LoginPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

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
        
        public LoginPanelViewModel(INavigationService navigationService, IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            ResponseText = string.Empty;
            TalkingToServer = false;
            
            LoginCommand = ReactiveCommand.Create(PerformLogin);
            GoToRegisterCommand = ReactiveCommand.Create(GoToRegister);
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
            
            var resp = await _authenticationService.TryLogin(Username, Password);
            if (resp.success)
            {
                _authenticationService.CurrentUser = resp.user;
                
                TalkingToServer = false;
                _navigationService.Navigate<MainPanelViewModel>();
                return;
            }
            
            ResponseText = resp.message;
            TalkingToServer = false;
        }

        private void GoToRegister()
        {
            _navigationService.Navigate<RegisterPanelViewModel>();
        }
    }
}