
namespace ChatAppFrontEnd.ViewModels
{
    public class SidebarViewModel : ViewModelBase
    {
        public ViewModelBase BottomViewModel { get; set; }
        
        public SidebarViewModel(SidebarBottomViewModel bottomViewModel)
        {
            BottomViewModel = bottomViewModel;
        }
    }
}
