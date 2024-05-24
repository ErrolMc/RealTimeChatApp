
using System.Collections.ObjectModel;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        private readonly IFriendService _friendService;
        
        private ObservableCollection<FriendListItemViewModel> _friends;
        
        public ObservableCollection<FriendListItemViewModel> Friends
        {
            get => _friends;
            set => this.RaiseAndSetIfChanged(ref _friends, value);
        } 

        public SidebarBottomViewModel BottomViewModel { get; set; }
        
        public SidebarViewModel(SidebarBottomViewModel bottomViewModel, IFriendService friendService)
        {
            BottomViewModel = bottomViewModel;
            _friendService = friendService;
        }

        public async void Setup()
        {
            Friends = new ObservableCollection<FriendListItemViewModel>();
            
            await _friendService.UpdateFriendsList();
            if (_friendService?.Friends == null)
                return;
            
            foreach (var friend in _friendService.Friends)
            {
                Friends.Add(new FriendListItemViewModel(friend, OpenChat));
            }
        }

        private void OpenChat(FriendListItemViewModel friendListItem)
        {
            
        }
    }
}
