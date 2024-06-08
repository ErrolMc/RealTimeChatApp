using System;
using Avalonia.Input;
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
            if (message.FromUser.UserID != _otherUser.UserID)
                return;
            
            ChatHistoryViewModel.CreateMessage(message.FromUser.UserName, message.MessageContents);
        }

        public async void ShowChat(UserSimple user)
        {
            _otherUser = user;
            
            await ChatHistoryViewModel.Setup(user);
            ChatTopBarViewModel.Setup(user);
            
            IsShown = true;
        }

        public async void SendMessage()
        {
            if (_sendingMessage)
                return;
            _sendingMessage = true;
            
            bool res = await _chatService.SendDirectMessage(_otherUser.UserID, MessageBoxText);
            if (res == false)
            {
                Console.WriteLine("Cant send message");
            }
            
            ChatHistoryViewModel.CreateMessage(_authenticationService.CurrentUser.Username, MessageBoxText);
            MessageBoxText = "";
            
            _sendingMessage = false;
        }
    }
}
