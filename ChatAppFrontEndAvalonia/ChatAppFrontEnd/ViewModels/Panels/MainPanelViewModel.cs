using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MainPanelViewModel : PanelViewModelBase
    {
        private ChatViewModel _chatView;
        private SidebarViewModel _sideBarView;

        public ChatViewModel ChatView
        {
            get => _chatView;
            set => this.RaiseAndSetIfChanged(ref _chatView, value);
        }
        
        public SidebarViewModel SideBarView
        {
            get => _sideBarView;
            set => this.RaiseAndSetIfChanged(ref _sideBarView, value);
        }

        public MainPanelViewModel(SidebarViewModel sideBar, ChatViewModel chatView)
        {
            ChatView = chatView;
            SideBarView = sideBar;
        }

        public override void OnShow()
        {
            _sideBarView.Setup();
        }
    }
}

