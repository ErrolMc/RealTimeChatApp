using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class RegisterPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        
        private string _username;
        private string _password;
        private string _confirmPassword;
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

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
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
        
        public ICommand GotoLoginCommand { get; }
        public ICommand RegisterCommand { get; }
        
        public RegisterPanelViewModel(INavigationService navigationService, IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            
            TalkingToServer = false;
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            
            GotoLoginCommand = ReactiveCommand.Create(GotoLogin);
            RegisterCommand = ReactiveCommand.Create(Register);
        }

        private void GotoLogin()
        {
            if (!TalkingToServer)
               _navigationService.Navigate<LoginPanelViewModel>();
        }

        private async void Register()
        {
            if (TalkingToServer) return;
            TalkingToServer = true;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(ConfirmPassword))
            {
                ResponseText = "Please fill in all fields!";
                TalkingToServer = false;
                return;
            }
            
            if (Password.Equals(ConfirmPassword) == false)
            {
                ResponseText = "Passwords dont match!";
                TalkingToServer = false;
                return;
            }

            ResponseText = "Trying to register...";
            
            var resp = await _authenticationService.TryRegister(Username, Password);
            if (resp.success)
            {
                TalkingToServer = false;
                GotoLogin();
                return;
            }

            ResponseText = resp.message;
            TalkingToServer = false;
        }
    }
}

