using System;
using Avalonia.Controls;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Other;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private ChatHistoryViewModel _chatHistoryViewModel;
        public ChatHistoryViewModel ChatHistoryViewModel
        {
            get => _chatHistoryViewModel;
            set => this.RaiseAndSetIfChanged(ref _chatHistoryViewModel, value);
        }
        
        private ChatTopBarViewModel _chatTopBarViewModel;
        public ChatTopBarViewModel ChatTopBarViewModel
        {
            get => _chatTopBarViewModel;
            set => this.RaiseAndSetIfChanged(ref _chatTopBarViewModel, value);
        }
        
        private ChatSidebarViewModelBase _rightSidebarViewModel;
        public ChatSidebarViewModelBase RightSideBarViewModel
        {
            get => _rightSidebarViewModel;
            set => this.RaiseAndSetIfChanged(ref _rightSidebarViewModel, value);
        }
        
        private string _messageBoxText;
        public string MessageBoxText
        {
            get => _messageBoxText;
            set => this.RaiseAndSetIfChanged(ref _messageBoxText, value);
        }

        private bool _isShown;
        public bool IsShown
        {
            get => _isShown;
            set => this.RaiseAndSetIfChanged(ref _isShown, value);
        }
        
        private GridLength _sidebarWidth;
        public GridLength SidebarWidth
        {
            get => _sidebarWidth;
            set => this.RaiseAndSetIfChanged(ref _sidebarWidth, value);
        }

        private readonly IChatService _chatService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ChatSidebarViewModelFactory _sideBarFactory;
        
        private IChatEntity _chatEntity;
        private bool _sendingMessage;
        
        public ChatViewModel(ChatHistoryViewModel chatHistoryViewModel, IChatService chatService, IAuthenticationService authenticationService, ChatSidebarViewModelFactory sideBarFactory)
        {
            _chatService = chatService;
            _authenticationService = authenticationService;
            _sideBarFactory = sideBarFactory;
            
            IsShown = false;
            _sendingMessage = false;

            ShowSidebar(false);
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
            if (!_chatEntity.DoesMessageThreadMatch(message))
                return;
            
            ChatHistoryViewModel.CreateMessage(message.FromUser.UserName, message.MessageContents);
        }

        public async void ShowChat(IChatEntity chatEntity)
        {
            _chatEntity = chatEntity;
            MessageBoxText = string.Empty;
            
            await ChatHistoryViewModel.Setup(_chatEntity);
            
            RightSideBarViewModel = _sideBarFactory.GetViewModel(_chatEntity);
            if (RightSideBarViewModel != null)
            {
                await RightSideBarViewModel.Populate(_chatEntity);
                ShowSidebar(true);
            }
            else
                ShowSidebar(false);

            ChatTopBarViewModel.Setup(_chatEntity);
            IsShown = true;
        }

        public async void SendMessage()
        {
            if (_sendingMessage)
                return;
            _sendingMessage = true;
            
            bool res = await _chatService.SendMessage(_chatEntity, MessageBoxText);
            if (res == false)
            {
                Console.WriteLine("Cant send message");
                return;
            }

            ChatHistoryViewModel.CreateMessage(_authenticationService.CurrentUser.Username, MessageBoxText);
            MessageBoxText = "";
            
            _sendingMessage = false;
        }

        private void ShowSidebar(bool state)
        {
            SidebarWidth = new GridLength(state ? 200 : 0);
        }
    }
}
