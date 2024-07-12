using System.Threading.Tasks;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppFrontEnd.ViewModels
{
    public abstract class ChatSidebarViewModelBase : ViewModelBase
    {
        public abstract Task Populate(IChatEntity chatEntity);
    }
}
