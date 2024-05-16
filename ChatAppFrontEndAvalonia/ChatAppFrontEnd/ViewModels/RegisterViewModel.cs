using System;
using System.Windows.Input;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private Action _gotoLoginAction;
        private string _username;
        private string _password;
        private string _confirmPassword;

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
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }
        
        public ICommand GotoLoginCommand { get; }
        public ICommand RegisterCommand { get; }
        
        public RegisterViewModel(Action gotoLoginAction)
        {
            _gotoLoginAction = gotoLoginAction;
            
            GotoLoginCommand = ReactiveCommand.Create(GotoLogin);
            RegisterCommand = ReactiveCommand.Create(Register);
        }

        private void GotoLogin()
        {
            _gotoLoginAction.Invoke();
        }

        private async void Register()
        {
            
        }
    }
}

