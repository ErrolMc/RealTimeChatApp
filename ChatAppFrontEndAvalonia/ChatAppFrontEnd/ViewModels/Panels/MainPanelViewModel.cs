using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MainPanelViewModel : PanelViewModelBase
    {
        private readonly DMSidebarViewModel _dmSidebarView;
        private readonly ServerSidebarViewModel _serverSidebarView;
        
        private ChatViewModel _chatView;
        private ViewModelBase _sideBarView;
        private SidebarBottomViewModel _sideBarBottomView;

        public SidebarBottomViewModel SideBarBottomView
        {
            get => _sideBarBottomView;
            set => this.RaiseAndSetIfChanged(ref _sideBarBottomView, value);
        }
        
        public ChatViewModel ChatView
        {
            get => _chatView;
            set => this.RaiseAndSetIfChanged(ref _chatView, value);
        }
        
        public ViewModelBase SideBarView
        {
            get => _sideBarView;
            set => this.RaiseAndSetIfChanged(ref _sideBarView, value);
        }

        public MainPanelViewModel(DMSidebarViewModel dmSidebar, ServerSidebarViewModel serverSidebar, SidebarBottomViewModel sideBarBottom, ChatViewModel chatView)
        {
            _dmSidebarView = dmSidebar;
            _serverSidebarView = serverSidebar;

            SideBarView = _dmSidebarView;
            ChatView = chatView;
            SideBarBottomView = sideBarBottom;
        }

        public override void OnShow()
        {
            _chatView.OnShow();
            _dmSidebarView.Setup(OnSelectChat);
        }

        public override void OnHide()
        {
            _chatView.OnHide();
        }

        private void OnSelectChat(UserSimple user)
        {
            _chatView?.ShowChat(user);
        }
    }
}

