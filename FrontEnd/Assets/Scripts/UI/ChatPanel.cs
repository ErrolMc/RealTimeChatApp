using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;
using ChatApp.Services;
using ChatApp.Shared.Tables;

namespace ChatApp.UI
{
    public class ChatPanel : Panel
    {
        [SerializeField] private FriendsListUI friendsListUI;
        [SerializeField] private TextMeshProUGUI userNameText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ChatMessage templateMessage;

        //[Inject] private IBroadcastService _broadcastService;
        [Inject] private IAuthenticationService _authenticationService;

        private bool loadedUserData = false;
        
        public override void OnShow()
        {
            //_broadcastService.OnMessageReceived += ReceiveMessage;
            templateMessage.gameObject.SetActive(false);
            
            PopulateUserData(!loadedUserData);
            if (!loadedUserData)
            {
                loadedUserData = true;
            }
            
            base.OnShow();
        }

        private async void PopulateUserData(bool firstRun = false)
        {
            User user = _authenticationService.CurrentUser;
            if (user == null)
            {
                Debug.LogError("ChatPanel: Current user is null");
                return;
            }
            
            userNameText.text = user.Username;

            friendsListUI.OnShow();
            bool friendsResult = await friendsListUI.PopulateFriendsList(firstRun);
        }

        public override void OnHide()
        {
            friendsListUI.OnHide();
            base.OnHide();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string msg = inputField.text;
                if (!string.IsNullOrEmpty(msg))
                {
                    msg = $"{_authenticationService.CurrentUser.Username}: " + msg;
                    CreateMessage(msg);
                    //_broadcastService.BroadcastMessage(msg);
                    inputField.text = "";
                }
            }
        }

        public void ReceiveMessage(string message)
        {
            CreateMessage(message);
        }

        public void CreateMessage(string message)
        {
            ChatMessage chatMessage = Instantiate(templateMessage, templateMessage.transform.parent);
            chatMessage.SetMessageText(message);
            chatMessage.gameObject.SetActive(true);
        }
    }   
}
