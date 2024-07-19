using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using ChatApp.Services;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Other;
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

        private void OnClickItem(DMSidebarItemViewModel dmSidebarItem, bool isLeftClick, Point mousePos)
        {
            if (isLeftClick)
                _openChatAction?.Invoke(dmSidebarItem.ChatEntity);
            else
                ShowRightClickMenu(dmSidebarItem.ChatEntity, mousePos);
        }
        
        private void ShowRightClickMenu(IChatEntity chatEntity, Point mousePos)
        {
            List<RightClickMenuButtonViewModel> buttons = new List<RightClickMenuButtonViewModel>();
            switch (chatEntity)
            {
                case UserSimple user:
                    {
                        buttons.Add(new RightClickMenuButtonViewModel("Remove Friend", () =>
                        {
                            _overlayService.HideOverlay();
                            UnFriend(user);
                        }));
                    }
                    break;
                case GroupDMSimple groupDM:
                    {
                        buttons.Add(new RightClickMenuButtonViewModel("Leave Group", () =>
                        {
                            _overlayService.HideOverlay();
                            //_groupService.leavegroup
                        }));
                    }
                    break;
            }

            RightClickMenuViewModel menu = new RightClickMenuViewModel(buttons);
            
            // TODO: there will need to be a check here to see if at bottom of window, make it go up from the cursor instead of down, same goes if too far to the right
            _overlayService.ShowOverlay(menu, topOffset: mousePos.Y, leftOffset: mousePos.X, () => { }); 
        }
        
        #region groupdms
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
                GroupDMs.Add(new DMSidebarItemViewModel(groupDM, OnClickItem));
            }
        }
        #endregion
        
        #region friends
        private void RefreshFriendsList()
        {
            if (_friendService?.Friends == null)
                return;
            
            Friends.Clear();
            foreach (var friend in _friendService.Friends)
            {
                Friends.Add(new DMSidebarItemViewModel(friend, OnClickItem));
            }
        }

        private async void UnFriend(UserSimple user)
        {
            var response = await _friendService.UnFriend(user.UserID);
        }
        #endregion
        
        ~DMSidebarViewModel()
        {
            if (_groupService != null)
                _groupService.OnGroupDMsUpdated -= RefreshGroupDMs;
            if (_friendService != null)
                _friendService.FriendsListUpdated -= RefreshFriendsList;
        }
    }
}
