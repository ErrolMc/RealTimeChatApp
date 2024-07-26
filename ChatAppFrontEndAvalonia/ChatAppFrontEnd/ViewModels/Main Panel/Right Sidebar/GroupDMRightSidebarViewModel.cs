using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using System.Collections.ObjectModel;
using Avalonia;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;
using System.Collections.Generic;
using ChatApp.Shared.Enums;

namespace ChatAppFrontEnd.ViewModels
{
    public class GroupDMRightSidebarViewModel : ChatSidebarViewModelBase
    {
        private ObservableCollection<DMSidebarItemViewModel> _members;
        public ObservableCollection<DMSidebarItemViewModel> Members
        {
            get => _members;
            set => this.RaiseAndSetIfChanged(ref _members, value);
        }

        private readonly IGroupService _groupService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlayService _overlayService;
        
        private GroupDMSimple _groupDM;
        private UserSimple _tempUser;
        
        public GroupDMRightSidebarViewModel(IGroupService groupService, IAuthenticationService authenticationService, IOverlayService overlayService)
        {
            _groupService = groupService;
            _authenticationService = authenticationService;
            _overlayService = overlayService;
            
            Members = new ObservableCollection<DMSidebarItemViewModel>();
            
            if (_groupService != null)
                _groupService.OnGroupUpdated += RefreshGroupDM;
        }
        
        public override async Task Populate(IChatEntity chatEntity)
        {
            Members.Clear();
            
            if (chatEntity is not GroupDMSimple groupDM)
                return;

            _groupDM = groupDM;
            var resp = await _groupService.GetGroupParticipants(groupDM.GroupID);
            
            foreach (UserSimple user in resp.Participants)
                Members.Add(new DMSidebarItemViewModel(user, OnClick_User));
        }

        private void OnClick_User(DMSidebarItemViewModel viewModel, bool isLeftClick, Point mousePos)
        {
            if (viewModel.ChatEntity is not UserSimple user)
                return;
            
            if (!isLeftClick)
            {
                ShowRightClickMenu(user, mousePos);
                return;
            }
            
            // TODO: show the users profile or take to DM
        }

        #region right click
        private void ShowRightClickMenu(UserSimple user, Point mousePos)
        {
            _tempUser = user;
            List<RightClickMenuButtonViewModel> buttons = new List<RightClickMenuButtonViewModel>();

            string curUserID = _authenticationService.CurrentUser.UserID;
            bool isHost = _groupDM.Owner == curUserID;

            if (isHost)
            {
                if (user.UserID != curUserID)
                {
                    buttons.Add(new RightClickMenuButtonViewModel("Remove from group", (clickPos) =>
                    {
                        ViewModelBase confirmDialog = new ConfirmRemoveDialogViewModel($"Remove '{user.Name}'",
                            $"Are you sure you want remove {user.Name} from {_groupDM.Name}?",
                            "Remove from group",
                            KickFromGroup);
                        _overlayService.ShowOverlayCentered(confirmDialog, () => _overlayService.HideOverlay());
                    }));   
                }
            }
            else
            {
                // buttons if not host
            }
            
            RightClickMenuViewModel menu = new RightClickMenuViewModel(buttons);
            
            // TODO: there will need to be a check here to see if at bottom of window, make it go up from the cursor instead of down, same goes if too far to the right
            const int RIGHT_CLICK_MENU_WIDTH = 200;
            _overlayService.ShowOverlay(menu, topOffset: mousePos.Y, leftOffset: mousePos.X - RIGHT_CLICK_MENU_WIDTH, () => { });  // TODO: Get the width programatically in code
        }

        private async void KickFromGroup(bool confirmed)
        {
            _overlayService.HideOverlay();
            if (!confirmed || _tempUser == null)
                return;
            
            var response = await _groupService.RemoveUserFromGroup(_tempUser.UserID, _groupDM, GroupUpdateReason.UserKicked);
        }
        #endregion

        private async void RefreshGroupDM((GroupDMSimple groupDM, GroupUpdateReason reason) res)
        {
            if (res.reason.IsReasonToDeleteLocalGroup())
                return;
            
            await Populate(res.groupDM);
        }
        
        ~GroupDMRightSidebarViewModel()
        {
            if (_groupService != null)
                _groupService.OnGroupUpdated -= RefreshGroupDM;
        }
    }
}

