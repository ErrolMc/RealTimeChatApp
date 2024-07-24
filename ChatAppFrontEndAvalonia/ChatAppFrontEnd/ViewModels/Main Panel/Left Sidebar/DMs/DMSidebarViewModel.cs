using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using ChatApp.Services;
using ChatApp.Shared.Enums;
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
        private readonly IAuthenticationService _authenticationService;

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

        private IChatEntity _tempChatEntity;
        
        public DMSidebarViewModel(IFriendService friendService, IOverlayService overlayService, IGroupService groupService, IAuthenticationService authenticationService)
        {
            _friendService = friendService;
            _overlayService = overlayService;
            _groupService = groupService;
            _authenticationService = authenticationService;
            
            Friends = new ObservableCollection<DMSidebarItemViewModel>();
            GroupDMs = new ObservableCollection<DMSidebarItemViewModel>();

            if (_groupService != null)
            {
                _groupService.OnGroupDMsUpdated += RefreshGroupDMs;
                _groupService.OnGroupUpdated += RefreshGroupDM;
            }
            if (_friendService != null)
                _friendService.FriendsListUpdated += RefreshFriendsList;
            
            CreateGroupDMCommand = ReactiveCommand.Create(OnClick_CreateGroupDM);
        }

        public async void Setup(Action<IChatEntity> openChatAction)
        {
            _openChatAction = openChatAction;
            
            Friends?.Clear();
            GroupDMs?.Clear();
            
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
            _tempChatEntity = chatEntity;
            
            List<RightClickMenuButtonViewModel> buttons = new List<RightClickMenuButtonViewModel>();
            switch (chatEntity)
            {
                case UserSimple user:
                    {
                        buttons.Add(new RightClickMenuButtonViewModel("Remove Friend", () =>
                        {
                            ViewModelBase confirmDialog = new ConfirmRemoveDialogViewModel($"Remove '{user.UserName}'",
                                $"Are you sure you want to permanently remove {user.UserName} from your friends?",
                                "Remove Friend",
                                UnFriendSelectedUser);
                            _overlayService.ShowOverlayCentered(confirmDialog, () => _overlayService.HideOverlay());
                        }));
                    }
                    break;
                case GroupDMSimple groupDM:
                {
                    if (_authenticationService.CurrentUser == null)
                    {
                        Console.WriteLine("_authenticationService.CurrentUser is null");
                        return;
                    }
                    
                    bool isHost = groupDM.Owner == _authenticationService.CurrentUser.UserID;

                    if (isHost)
                    {
                        buttons.Add(new RightClickMenuButtonViewModel("Delete Group", () =>
                        {
                            ViewModelBase confirmDialog = new ConfirmRemoveDialogViewModel($"Delete '{groupDM.Name}'",
                                $"Are you sure you want delete the group {groupDM.Name}?",
                                "Delete Group",
                                DeleteGroup);
                            _overlayService.ShowOverlayCentered(confirmDialog, () => _overlayService.HideOverlay());
                        }));   
                    }
                    else
                    {
                        buttons.Add(new RightClickMenuButtonViewModel("Leave Group", () =>
                        {
                            ViewModelBase confirmDialog = new ConfirmRemoveDialogViewModel($"Leave '{groupDM.Name}'",
                                $"Are you sure you want leave the group {groupDM.Name}?",
                                "Leave Group",
                                LeaveGroup);
                            _overlayService.ShowOverlayCentered(confirmDialog, () => _overlayService.HideOverlay());
                        }));   
                    }
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
        
        private void RefreshGroupDM((GroupDMSimple groupDM, GroupUpdateReason reason) res)
        {
            DMSidebarItemViewModel groupItem = GroupDMs.FirstOrDefault(gp => gp.ChatEntity.ID == res.groupDM.ID);
            if (groupItem == null)
                return;

            if (res.reason is GroupUpdateReason.ThisUserLeft or GroupUpdateReason.ThisUserKicked)
            {
                GroupDMs.Remove(groupItem);
                return;
            }
            
            groupItem.Populate(res.groupDM);
        }
        
        private async void LeaveGroup(bool confirmed)
        {
            _overlayService.HideOverlay();
            if (!confirmed || _tempChatEntity is not GroupDMSimple groupDM)
                return;

            var response = await _groupService.RemoveUserFromGroup(_authenticationService.CurrentUser.UserID, groupDM, GroupUpdateReason.UserLeft);
        }
        
        private async void DeleteGroup(bool confirmed)
        {
            _overlayService.HideOverlay();
            if (!confirmed || _tempChatEntity is not GroupDMSimple groupDM)
                return;
            
            // delete group
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

        private async void UnFriendSelectedUser(bool confirmed)
        {
            _overlayService.HideOverlay();
            if (!confirmed || _tempChatEntity is not UserSimple user)
                return;
            
            var response = await _friendService.UnFriend(user.UserID);
        }
        #endregion
        
        ~DMSidebarViewModel()
        {
            if (_groupService != null)
            {
                _groupService.OnGroupDMsUpdated -= RefreshGroupDMs;
                _groupService.OnGroupUpdated -= RefreshGroupDM;
            }
            if (_friendService != null)
                _friendService.FriendsListUpdated -= RefreshFriendsList;
        }
    }
}
