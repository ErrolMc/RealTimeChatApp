using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels 
{
    public class SidebarBottomViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
    
        public ICommand FriendsCommand { get; }
        public ICommand SettingsCommand { get; }
    
        public SidebarBottomViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            FriendsCommand = ReactiveCommand.Create(GotoFriends);
            SettingsCommand = ReactiveCommand.Create(GotoSettings);;
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

