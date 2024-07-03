using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class DMSidebarViewModel : ViewModelBase
    {
        private readonly IFriendService _friendService;
        private readonly IOverlayService _overlayService;

        private System.Action<UserSimple> _openChatAction;
        private ObservableCollection<DMSidebarItemViewModel> _friends;
        private ObservableCollection<DMSidebarItemViewModel> _groupDMs;
        
        public ICommand CreateGroupDMCommand { get; }
        
        public ObservableCollection<DMSidebarItemViewModel> Friends
        {
            get => _friends;
            set => this.RaiseAndSetIfChanged(ref _friends, value);
        } 
        
        public ObservableCollection<DMSidebarItemViewModel> GroupDMs
        {
            get => _groupDMs;
            set => this.RaiseAndSetIfChanged(ref _groupDMs, value);
        } 
        
        public DMSidebarViewModel(IFriendService friendService, IOverlayService overlayService)
        {
            _friendService = friendService;
            _overlayService = overlayService;

            CreateGroupDMCommand = ReactiveCommand.Create(OnClick_CreateGroupDM);
        }

        public async void Setup(System.Action<UserSimple> openChatAction)
        {
            _openChatAction = openChatAction;
            Friends = new ObservableCollection<DMSidebarItemViewModel>();
            
            await _friendService.UpdateFriendsList();
            if (_friendService?.Friends == null)
                return;
            
            foreach (var friend in _friendService.Friends)
            {
                Friends.Add(new DMSidebarItemViewModel(friend, OpenChat));
            }

            GroupDMs = new ObservableCollection<DMSidebarItemViewModel>()
            {
                new DMSidebarItemViewModel("Group 1", OpenChat),
                new DMSidebarItemViewModel("Group 2", OpenChat),
                new DMSidebarItemViewModel("Group 3", OpenChat),
                new DMSidebarItemViewModel("Group 4", OpenChat),
                new DMSidebarItemViewModel("Group 5", OpenChat),
                new DMSidebarItemViewModel("Group 6", OpenChat),
                new DMSidebarItemViewModel("Group 7", OpenChat),
                new DMSidebarItemViewModel("Group 8", OpenChat),
                new DMSidebarItemViewModel("Group 9", OpenChat),
                new DMSidebarItemViewModel("Group 10", OpenChat),
                new DMSidebarItemViewModel("Group 11", OpenChat),
                new DMSidebarItemViewModel("Group 12", OpenChat),
            };
        }

        private void OpenChat(DMSidebarItemViewModel dmSidebarItem)
        {
            if (dmSidebarItem.IsGroupDM)
            {
                // group dm stuff
                return;
            }
            _openChatAction?.Invoke(dmSidebarItem.User);
        }

        private void OnClick_CreateGroupDM()
        {
            _overlayService.ShowOverlay(new CreateGroupDMViewModel(_friendService.Friends), 60, 130, () =>
            {
                // cancel create group dm
            });
        }
    }
}
