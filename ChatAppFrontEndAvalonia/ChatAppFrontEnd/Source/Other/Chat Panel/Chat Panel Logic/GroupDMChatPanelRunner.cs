using ChatApp.Shared.Enums;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Services;

namespace ChatAppFrontEnd.Source.ChatPanel
{
    public class GroupDMChatPanelRunner : ChatPanelRunnerBase
    {
        private readonly IGroupService _groupService;
        private readonly IChatService _chatService;
        
        public GroupDMChatPanelRunner(IGroupService groupService, IChatService chatService)
        {
            _groupService = groupService;
            _chatService = chatService;
        }

        private void OnGroupUpdated((GroupDMSimple groupDM, GroupUpdateReason reason) res)
        {
            if (_chatService.CurrentChat.ID != res.groupDM.GroupID)
                return;

            if (res.reason.IsReasonToDeleteLocalGroup())
                HideChat();
            else
                PopulateTopBar(res.groupDM);
        }
        
        public override void RegisterEvents()
        {
            if (_groupService != null)
                _groupService.OnGroupUpdated += OnGroupUpdated;
        }

        public override void UnRegisterEvents()
        {
            if (_groupService != null)
                _groupService.OnGroupUpdated -= OnGroupUpdated;
        }
    }
}
