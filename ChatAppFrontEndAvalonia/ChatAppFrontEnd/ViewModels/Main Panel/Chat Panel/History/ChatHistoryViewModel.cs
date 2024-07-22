using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatHistoryViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        
        private ObservableCollection<ChatMessageViewModel> _messages;
        
        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }

        public ChatHistoryViewModel(IChatService chatService)
        {
            _chatService = chatService;
            
            Messages = new ObservableCollection<ChatMessageViewModel>();
        }
        
        public async Task Setup(IChatEntity chatEntity)
        {
            ClearMessages();
            
            List<Message> respMessages = await _chatService.GetMessages(chatEntity);
            foreach (Message messageData in respMessages)
                CreateMessage(messageData.FromUser.UserName, messageData.MessageContents);
        }

        public void CreateMessage(string fromUserName, string contents)
        {
            Messages.Add(new ChatMessageViewModel(fromUserName, contents));
        }

        public void ClearMessages()
        {
            Messages.Clear();
        }
    }
}
