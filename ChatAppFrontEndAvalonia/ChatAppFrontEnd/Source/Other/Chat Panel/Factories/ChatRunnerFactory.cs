using ChatApp.Shared.TableDataSimple;
using ChatAppFrontEnd.Source.ChatPanel;

namespace ChatAppFrontEnd.Source.Other
{
    public class ChatRunnerFactory
    {
        private readonly FriendChatPanelRunner _friendRunner;
        private readonly GroupDMChatPanelRunner _groupDMRunner;
        private readonly ServerChatPanelRunner _serverRunner;
        
        public ChatRunnerFactory(FriendChatPanelRunner friendRunner, GroupDMChatPanelRunner groupDMRunner, ServerChatPanelRunner serverRunner)
        {
            _friendRunner = friendRunner;
            _groupDMRunner = groupDMRunner;
            _serverRunner = serverRunner;
        }
        
        public ChatPanelRunnerBase GetRunnerForChatEntity(IChatEntity chatEntity)
        {
            switch (chatEntity)
            {
                case UserSimple user: return _friendRunner;
                case GroupDMSimple groupDM: return _groupDMRunner;
                // server stuff
            }

            return null;
        }

        public bool AreRunnersDifferent(ChatPanelRunnerBase runner, IChatEntity chatEntity)
        {
            return runner != GetRunnerForChatEntity(chatEntity);
        }
    }
}
