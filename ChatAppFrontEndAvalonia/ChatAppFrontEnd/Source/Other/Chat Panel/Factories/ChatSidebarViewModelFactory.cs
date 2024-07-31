using ChatApp.Services;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Source.Other
{
    public class ChatSidebarViewModelFactory
    {
        private readonly IFriendService _friendService;
        private readonly IGroupService _groupService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOverlayService _overlayService;
        
        public ChatSidebarViewModelFactory(IFriendService friendService, IGroupService groupService, IAuthenticationService authenticationService, IOverlayService overlayService)
        {
            _friendService = friendService;
            _groupService = groupService;
            _authenticationService = authenticationService;
            _overlayService = overlayService;
        }
        
        public ChatSidebarViewModelBase GetViewModel(IChatEntity chatEntity)
        {
            switch (chatEntity)
            {
                //case UserSimple user: return new FriendDMRightSidebarViewModel(_friendService);
                case UserSimple: return null; // for now no sidebar
                case GroupDMSimple groupDM: return new GroupDMRightSidebarViewModel(_groupService, _authenticationService, _overlayService);
                // server sidebar
            }
            return null;
        }
    }
}
