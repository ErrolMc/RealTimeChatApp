using System;
using Avalonia.Controls;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.ChatPanel;
using ChatAppFrontEnd.Source.Other;
using ChatAppFrontEnd.Source.Other.Caching.Data;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        #region view bindings
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
        #endregion

        private readonly IChatService _chatService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ChatRunnerFactory _runnerFactory;
        private readonly ChatSidebarViewModelFactory _sideBarFactory;

        private ChatPanelRunnerBase _currentRunner;
        private IChatEntity _chatEntity;
        private bool _sendingMessage;
        
        public ChatViewModel(ChatHistoryViewModel chatHistoryViewModel, IChatService chatService, IAuthenticationService authenticationService, ChatRunnerFactory runnerFactory, ChatSidebarViewModelFactory sideBarFactory)
        {
            _chatService = chatService;
            _authenticationService = authenticationService;
            _runnerFactory = runnerFactory;
            _sideBarFactory = sideBarFactory;

            IsShown = false;
            _sendingMessage = false;

            ShowSidebar(false);
            MessageBoxText = "";
            ChatHistoryViewModel = chatHistoryViewModel;
            ChatTopBarViewModel = new ChatTopBarViewModel();
            
            _authenticationService.OnLogout += OnAuthLogout;
        }
        
        public async void ShowChat(IChatEntity chatEntity)
        {
            // set variables
            _chatEntity = chatEntity;
            _chatService.CurrentChat = chatEntity;
            
            // setup chat runner (logic for handling events etc)
            if (_runnerFactory.AreRunnersDifferent(_currentRunner, _chatEntity))
            {
                _currentRunner?.UnRegisterEvents();
                _currentRunner = _runnerFactory.GetRunnerForChatEntity(chatEntity);
                _currentRunner?.Setup(HideChat, PopulateTopBar);
                _currentRunner?.RegisterEvents();
            }
            
            // setup chat history
            MessageBoxText = string.Empty;
            await ChatHistoryViewModel.Setup(_chatEntity);
            
            // setup sidebar
            RightSideBarViewModel = _sideBarFactory.GetViewModel(_chatEntity);
            if (RightSideBarViewModel != null)
            {
                await RightSideBarViewModel.Populate(_chatEntity);
                ShowSidebar(true);
            }
            else
                ShowSidebar(false);

            // setup top bar
            PopulateTopBar(_chatEntity);
            
            // mark shown
            IsShown = true;
        }
        
        private void ShowSidebar(bool state)
        {
            SidebarWidth = new GridLength(state ? 200 : 0);
        }

        #region creating messages
        public async void SendMessage()
        {
            if (_sendingMessage || string.IsNullOrEmpty(MessageBoxText))
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
        
        private void OnReceiveMessage(MessageCache message)
        {
            if (!_chatEntity.DoesMessageThreadMatch(message))
                return;
            
            ChatHistoryViewModel.CreateMessage(message.FromUser.UserName, message.Message);
        }
        #endregion
        
        #region runner events
        private void HideChat()
        {
            ShowSidebar(false);
            ChatHistoryViewModel.ClearMessages();
            IsShown = false;
            ChatTopBarViewModel.IsShown = false;
        }

        private void PopulateTopBar(IChatEntity chatEntity)
        {
            ChatTopBarViewModel?.Setup(chatEntity);
        }
        #endregion

        #region Show/Hide events
        public void OnShow()
        {
            if (_chatService != null)
                _chatService.OnMessageReceived += OnReceiveMessage;
            _currentRunner?.RegisterEvents();
        }

        public void OnHide()
        {
            if (_chatService != null)
                _chatService.OnMessageReceived -= OnReceiveMessage;
            _currentRunner?.UnRegisterEvents();
        }
        
        private void OnAuthLogout()
        {
            _currentRunner?.UnRegisterEvents();
            _currentRunner = null;
            _chatEntity = null;
            _chatService.CurrentChat = null;
            HideChat();
        }
        #endregion
    }
}
