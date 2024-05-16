using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels 
{
    public class SidebarBottomViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;

        private string _nameText;
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }
    
        public ICommand FriendsCommand { get; }
        public ICommand SettingsCommand { get; }
    
        public SidebarBottomViewModel(INavigationService navigationService, IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            FriendsCommand = ReactiveCommand.Create(GotoFriends);
            SettingsCommand = ReactiveCommand.Create(GotoSettings);

            if (_authenticationService.CurrentUser != null)
                NameText = _authenticationService.CurrentUser.Username;
            else
                NameText = "No name found!";
        }

        private void GotoFriends()
        {
            _navigationService.Navigate<FriendsPanelViewModel>();
        }

        private void GotoSettings()
        {
            _navigationService.Navigate<SettingsPanelViewModel>();
        }
    }
}

