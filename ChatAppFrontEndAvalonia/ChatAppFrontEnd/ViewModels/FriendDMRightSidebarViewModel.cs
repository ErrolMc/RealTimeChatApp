using System.Threading.Tasks;
using ChatApp.Services;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendDMRightSidebarViewModel : ChatSidebarViewModelBase
    {
        private readonly IFriendService _friendService;
        
        public FriendDMRightSidebarViewModel(IFriendService friendService)
        {
            _friendService = friendService;
        }
        
        public override async Task Populate(IChatEntity chatEntity)
        {
            await Task.CompletedTask;
        }
    }
}

