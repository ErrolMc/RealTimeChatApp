using System;
using System.Windows.Input;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class LoginViewModel : PanelViewModelBase
    {
        private Action _gotoChatAction;
        private Action _gotoRegisterAction;
        private string _username;
        private string _password;

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

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        
        public LoginViewModel(Action gotoRegisterAction, Action gotoChatAction)
        {
            _gotoRegisterAction = gotoRegisterAction;
            _gotoChatAction = gotoChatAction;
            
            LoginCommand = ReactiveCommand.Create(PerformLogin);
            GoToRegisterCommand = ReactiveCommand.Create(GoToRegister);
        }

        private void PerformLogin()
        {
            _gotoChatAction.Invoke();
        }

        private void GoToRegister()
        {
            _gotoRegisterAction?.Invoke();
        }
    }
}