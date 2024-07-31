using System;
using ChatApp.Shared.TableDataSimple;

namespace ChatAppFrontEnd.Source.ChatPanel
{
    public class ChatPanelRunnerBase
    {
        private Action _hideChatAction;
        private Action<IChatEntity> _populateTopBarAction;
        
        public void Setup(Action hideChatAction, Action<IChatEntity> populateTopBarAction)
        {
            _hideChatAction = hideChatAction;
            _populateTopBarAction = populateTopBarAction;
        }

        protected void HideChat()
        {
            _hideChatAction?.Invoke();
        }

        protected void PopulateTopBar(IChatEntity chatEntity)
        {
            _populateTopBarAction?.Invoke(chatEntity);
        }
        
        public virtual void RegisterEvents() { }
        public virtual void UnRegisterEvents() { }
    }
}
