using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace ChatApp.UI
{
    public class ChatHistory : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ChatMessage templateMessage;

        [Inject] private IAuthenticationService _authenticationService;
        [Inject] private IChatService _chatService;

        public UserSimple OtherUser { get; set; }
        private bool _shown = false;
        private bool _sendingMessage = false;

        private List<ChatMessage> _messageItems;
        
        public async UniTask<bool> OnShow(UserSimple user)
        {
            if (_authenticationService?.CurrentUser == null)
                return false;

            ClearMessages();
            
            templateMessage.gameObject.SetActive(false);
            _chatService.OnMessageReceived += OnReceiveMessage;
            
            OtherUser = user;
            List<Message> messages = await _chatService.GetDirectMessages(_authenticationService.CurrentUser.UserID, user.UserID);
            foreach (Message message in messages)
            {
                CreateMessage(message.FromUser, message.MessageContents);
            }
            
            _shown = true;
            return true;
        }

        public void OnHide()
        {
            _chatService.OnMessageReceived -= OnReceiveMessage;
            ClearMessages();
            _shown = false;
        }
        
        private void Update()
        {
            if (!_shown)
                return;
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string msg = inputField.text;
                if (!string.IsNullOrEmpty(msg) && !_sendingMessage)
                {
                    SendMessage(msg);
                    inputField.text = "";
                }
            }
        }

        private async void SendMessage(string message)
        {
            if (_sendingMessage)
                return;
            _sendingMessage = true;

            UserSimple loggedInUserSimple = _authenticationService.CurrentUser.ToUserSimple();
            
            bool res = await _chatService.SendDirectMessage(loggedInUserSimple.UserID, OtherUser.UserID, message);

            if (res == false)
            {
                Debug.LogError("ChatHistory: Cant send message");
                return;
            }   
            
            CreateMessage(loggedInUserSimple, message);
            
            _sendingMessage = false;
        }
        
        private void CreateMessage(UserSimple fromUser, string message)
        {
            ChatMessage chatMessage = Instantiate(templateMessage, templateMessage.transform.parent);
            chatMessage.Setup(fromUser, message);
            chatMessage.gameObject.SetActive(true);
            
            _messageItems.Add(chatMessage);
        }

        public void OnReceiveMessage(Message message)
        {
            if (message.FromUser.UserID != OtherUser.UserID)
                return;
            
            CreateMessage(message.FromUser, message.MessageContents);
        }

        private void ClearMessages()
        {
            if (_messageItems != null)
            {
                for (int i = 0; i < _messageItems.Count; i++)
                    Destroy(_messageItems[i].gameObject);
                _messageItems.Clear();
            }
            else
                _messageItems = new List<ChatMessage>();
        }
    }
}
