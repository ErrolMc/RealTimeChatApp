using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatApp.Services;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class DMSidebarViewModel : ViewModelBase
    {
        private readonly IFriendService _friendService;
        private readonly IOverlayService _overlayService;
        private readonly IGroupService _groupService;

        private Action<IChatEntity> _openChatAction;
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
        
        public DMSidebarViewModel(IFriendService friendService, IOverlayService overlayService, IGroupService groupService)
        {
            _friendService = friendService;
            _overlayService = overlayService;
            _groupService = groupService;

            if (_groupService != null)
                _groupService.OnGroupDMsUpdated += RefreshGroupDMs;
            if (_friendService != null)
                _friendService.FriendsListUpdated += RefreshFriendsList;
            
            CreateGroupDMCommand = ReactiveCommand.Create(OnClick_CreateGroupDM);
        }

        public async void Setup(Action<IChatEntity> openChatAction)
        {
            _openChatAction = openChatAction;
            
            Friends = new ObservableCollection<DMSidebarItemViewModel>();
            GroupDMs = new ObservableCollection<DMSidebarItemViewModel>();

            if (await _friendService.UpdateFriendsList())
                RefreshFriendsList();
            
            if (await _groupService.UpdateGroupDMList())
                RefreshGroupDMs();
        }

        private void OpenChat(DMSidebarItemViewModel dmSidebarItem)
        {
            _openChatAction?.Invoke(dmSidebarItem.ChatEntity);
        }

        private void OnClick_CreateGroupDM()
        {
            _overlayService.ShowOverlay(new CreateGroupDMViewModel(_friendService.Friends, _groupService, OnCreateGroupSuccess), 60, 130,() => { });
        }
        
        private void OnCreateGroupSuccess(GroupDMSimple groupDMSimple)
        {
            _overlayService.HideOverlay();
            _groupService?.AddGroupLocally(groupDMSimple);
        }

        private void RefreshGroupDMs()
        {
            if (_groupService?.GroupDMs == null)
                return;
            
            GroupDMs.Clear();
            foreach (var groupDM in _groupService.GroupDMs)
            {
                GroupDMs.Add(new DMSidebarItemViewModel(groupDM, OpenChat));
            }
        }
        
        private void RefreshFriendsList()
        {
            if (_friendService?.Friends == null)
                return;
            
            Friends.Clear();
            foreach (var friend in _friendService.Friends)
            {
                Friends.Add(new DMSidebarItemViewModel(friend, OpenChat));
            }
        }

        ~DMSidebarViewModel()
        {
            if (_groupService != null)
                _groupService.OnGroupDMsUpdated -= RefreshGroupDMs;
            if (_friendService != null)
                _friendService.FriendsListUpdated -= RefreshFriendsList;
        }
    }
}
