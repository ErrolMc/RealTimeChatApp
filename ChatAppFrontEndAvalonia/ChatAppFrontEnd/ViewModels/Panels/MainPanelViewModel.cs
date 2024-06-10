using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MainPanelViewModel : PanelViewModelBase
    {
        private ChatViewModel _chatView;
        private FriendSidebarViewModel _sideBarView;
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
        
        public FriendSidebarViewModel SideBarView
        {
            get => _sideBarView;
            set => this.RaiseAndSetIfChanged(ref _sideBarView, value);
        }

        public MainPanelViewModel(FriendSidebarViewModel sideBar, SidebarBottomViewModel sideBarBottom, ChatViewModel chatView)
        {
            ChatView = chatView;
            SideBarView = sideBar;
            SideBarBottomView = sideBarBottom;
        }

        public override void OnShow()
        {
            _chatView.OnShow();
            _sideBarView.Setup(OnSelectChat);
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

