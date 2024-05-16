using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MainPanelViewModel : PanelViewModelBase
    {
        private ViewModelBase _chatView;
        private ViewModelBase _sideBarView;

        public ViewModelBase ChatView
        {
            get => _chatView;
            set => this.RaiseAndSetIfChanged(ref _chatView, value);
        }
        
        public ViewModelBase SideBarView
        {
            get => _sideBarView;
            set => this.RaiseAndSetIfChanged(ref _sideBarView, value);
        }

        public MainPanelViewModel(SidebarViewModel sideBar, ChatViewModel chatView)
        {
            ChatView = chatView;
            SideBarView = sideBar;
        }
    }
}

