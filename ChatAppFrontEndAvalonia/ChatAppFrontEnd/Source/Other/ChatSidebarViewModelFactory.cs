using ChatApp.Services;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Source.Other
{
    public class ChatSidebarViewModelFactory
    {
        private readonly IFriendService _friendService;
        private readonly IGroupService _groupService;
        
        public ChatSidebarViewModelFactory(IFriendService friendService, IGroupService groupService)
        {
            _friendService = friendService;
            _groupService = groupService;
        }
        
        public ChatSidebarViewModelBase GetViewModel(IChatEntity chatEntity)
        {
            switch (chatEntity)
            {
                //case UserSimple user: return new FriendDMRightSidebarViewModel(_friendService);
                case UserSimple: return null; // for now no sidebar
                case GroupDMSimple groupDM: return new GroupDMRightSidebarViewModel(_groupService);
                // server sidebar
            }
            return null;
        }
    }
}

