using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatHistoryViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<ChatMessageViewModel> _messages;
        
        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get => _messages;
            set => this.RaiseAndSetIfChanged(ref _messages, value);
        }

        public ChatHistoryViewModel(IChatService chatService, IAuthenticationService authenticationService)
        {
            _chatService = chatService;
            _authenticationService = authenticationService;
        }

        public async Task Setup(UserSimple user)
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();
            
            List<Message> respMessages = await _chatService.GetDirectMessages(_authenticationService.CurrentUser.UserID, user.UserID);
            foreach (Message messageData in respMessages)
                CreateMessage(messageData.FromUser.UserName, messageData.MessageContents);
        }

        public void CreateMessage(string fromUserName, string contents)
        {
            Messages.Add(new ChatMessageViewModel(fromUserName, contents));
        }

        public void ClearMessages()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();
        }
    }
}
