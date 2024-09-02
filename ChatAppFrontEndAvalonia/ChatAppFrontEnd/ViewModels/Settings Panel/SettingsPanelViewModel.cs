using System;
using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class SettingsPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        
        public ICommand BackCommand { get; }
        public ICommand LogoutCommand { get; }

        private bool _loggingOut = false;

        public SettingsPanelViewModel(INavigationService navigationService, IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            
            BackCommand = ReactiveCommand.Create(GoBack);
            LogoutCommand = ReactiveCommand.Create(Logout);
        }

        public override void OnShow()
        {
            _loggingOut = false;
        }

        private void GoBack()
        {
            if (!_loggingOut)
                _navigationService.GoBack();
        }

        private async void Logout()
        {
            _loggingOut = true;
            bool loggedOut = await _authenticationService.TryLogout();
            _navigationService.Navigate<LoginPanelViewModel>();
        }
    }
}
