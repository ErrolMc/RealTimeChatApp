using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using System.Collections.ObjectModel;
using Avalonia;
using ReactiveUI;

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
        
        public GroupDMRightSidebarViewModel(IGroupService groupService)
        {
            _groupService = groupService;
            Members = new ObservableCollection<DMSidebarItemViewModel>();
        }
        
        public override async Task Populate(IChatEntity chatEntity)
        {
            Members.Clear();
            
            if (chatEntity is not GroupDMSimple groupDM)
                return;
            
            var resp = await _groupService.GetGroupParticipants(groupDM.GroupID);
            
            foreach (UserSimple user in resp.Participants)
                Members.Add(new DMSidebarItemViewModel(user, OnClick_User));
        }

        private void OnClick_User(DMSidebarItemViewModel viewModel, bool isLeftClick, Point mousePos)
        {
            if (viewModel.ChatEntity is not UserSimple user)
                return;
            
            // TODO: show the users profile
        }
    }
}

