using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;

namespace ChatAppFrontEnd.ViewModels
{
    public class GroupDMRightSidebarViewModel : ChatSidebarViewModelBase
    {
        private readonly IGroupService _groupService;
        
        public GroupDMRightSidebarViewModel(IGroupService groupService)
        {
            _groupService = groupService;
        }
        
        public override async Task Populate(IChatEntity chatEntity)
        {
            await Task.CompletedTask;
        }
    }
}

