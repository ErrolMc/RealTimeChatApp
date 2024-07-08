using System;
using Avalonia.Input;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IChatService _chatService;
        private readonly IAuthenticationService _authenticationService;
        
        private ChatHistoryViewModel _chatHistoryViewModel;
        private ChatTopBarViewModel _chatTopBarViewModel;
        
        public ChatHistoryViewModel ChatHistoryViewModel
        {
            get => _chatHistoryViewModel;
            set => this.RaiseAndSetIfChanged(ref _chatHistoryViewModel, value);
        }
        
        public ChatTopBarViewModel ChatTopBarViewModel
        {
            get => _chatTopBarViewModel;
            set => this.RaiseAndSetIfChanged(ref _chatTopBarViewModel, value);
        }

        private UserSimple _otherUser;
        private GroupDMSimple _groupDM;
        private bool _isGroupDM;
        
        private bool _sendingMessage;
        private string _messageBoxText;
        private bool _isShown;
        
        public string MessageBoxText
        {
            get => _messageBoxText;
            set => this.RaiseAndSetIfChanged(ref _messageBoxText, value);
        }

        public bool IsShown
        {
            get => _isShown;
            set => this.RaiseAndSetIfChanged(ref _isShown, value);
        }

        public ChatViewModel(ChatHistoryViewModel chatHistoryViewModel, IChatService chatService, IAuthenticationService authenticationService)
        {
            _chatService = chatService;
            _authenticationService = authenticationService;
            
            IsShown = false;
            _sendingMessage = false;
            
            MessageBoxText = "";
            ChatHistoryViewModel = chatHistoryViewModel;
            ChatTopBarViewModel = new ChatTopBarViewModel();
        }

        public void OnShow()
        {
            _chatService.OnMessageReceived += OnReceiveMessage;
        }

        public void OnHide()
        {
            _chatService.OnMessageReceived -= OnReceiveMessage;
        }

        private void OnReceiveMessage(Message message)
        {
            if (_isGroupDM)
            {
                if (message.ThreadID != _groupDM.GroupID)
                    return;
            }
            else if (message.FromUser.UserID != _otherUser.UserID)
                return;
            
            ChatHistoryViewModel.CreateMessage(message.FromUser.UserName, message.MessageContents);
        }

        public async void ShowChat(UserSimple user)
        {
            _isGroupDM = false;
            _otherUser = user;
            
            await ChatHistoryViewModel.Setup(user);
            ChatTopBarViewModel.Setup(user);
            
            IsShown = true;
        }
        
        public async void ShowChat(GroupDMSimple groupDM)
        {
            _isGroupDM = true;
            _groupDM = groupDM;
            
            await ChatHistoryViewModel.Setup(groupDM);
            ChatTopBarViewModel.Setup(groupDM);
            
            IsShown = true;
        }

        public async void SendMessage()
        {
            if (_sendingMessage)
                return;
            _sendingMessage = true;

            if (_isGroupDM)
            {
                bool res = await _chatService.SendGroupDMMessage(_groupDM.GroupID, MessageBoxText);
                if (res == false)
                {
                    Console.WriteLine("Cant send message");
                    return;
                }
            }
            else
            {
                bool res = await _chatService.SendDirectMessage(_otherUser.UserID, MessageBoxText);
                if (res == false)
                {
                    Console.WriteLine("Cant send message");
                    return;
                }
            }
            
            ChatHistoryViewModel.CreateMessage(_authenticationService.CurrentUser.Username, MessageBoxText);
            MessageBoxText = "";
            
            _sendingMessage = false;
        }
    }
}
