using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;

namespace ChatApp.UI
{
    public class ChatPanel : Panel
    {
        [Header("Sidebar")]
        [SerializeField] private FriendsListUI friendsListUI;
        [SerializeField] private TextMeshProUGUI userNameText;

        [Header("Chat Window")] 
        [SerializeField] private ChatHistory chatHistory;
        [SerializeField] private UserDisplayItem friendInfoItem;
        
        [Inject] private IAuthenticationService _authenticationService;
        
        private bool loadedUserData = false;
        
        public override void OnShow()
        {
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

            friendsListUI.OnShow(OnSelectUser);
            bool friendsResult = await friendsListUI.PopulateFriendsList(firstRun);
            
            chatHistory.gameObject.SetActive(false);
        }

        private async void OnSelectUser(UserSimple user)
        {
            if (chatHistory.OtherUser == user)
                return;
            
            
            await chatHistory.OnShow(user);
            friendInfoItem.Setup(user);
            chatHistory.gameObject.SetActive(true);
        }
        
        public override void OnHide()
        {
            friendsListUI.OnHide();
            chatHistory.OnHide();
            
            base.OnHide();
        }
    }   
}
