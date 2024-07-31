using ChatApp.Services;
using ChatApp.Shared.Notifications;
using ChatAppFrontEnd.Source.Services;

namespace ChatAppFrontEnd.Source.ChatPanel
{
    public class FriendChatPanelRunner : ChatPanelRunnerBase
    {
        private readonly IFriendService _friendService;
        private readonly IChatService _chatService;
        
        public FriendChatPanelRunner(IFriendService friendService, IChatService chatService)
        {
            _friendService = friendService;
            _chatService = chatService;
        }

        private void OnRemoveFriend(UnfriendNotification notification)
        {
            if (_chatService.CurrentChat.ID == notification.FromUserID)
                HideChat();
        }
        
        public override void RegisterEvents()
        {
            if (_friendService != null)
                _friendService.OnUnfriended += OnRemoveFriend;
        }

        public override void UnRegisterEvents()
        {
            if (_friendService != null)
                _friendService.OnUnfriended -= OnRemoveFriend;
        }
    }
}

