using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatApp.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ChatApp.UI
{
    public class AddFriendPanel : Panel
    {
        [SerializeField] private TMP_InputField addFriendInputField;
        [SerializeField] private TextMeshProUGUI addFriendResponseText;
        [SerializeField] private Button addFriendButton;
        [SerializeField] private FriendRequestItem templateFriendRequestItem;
        [SerializeField] private FriendRequestItem templateOutgoingFriendRequestItem;
        
        private bool _talkingToServer = false;
        private List<FriendRequestItem> _friendRequestItems;
        private List<FriendRequestItem> _outgoingFriendRequestItems;
        
        [Inject] private IFriendService _friendService;
        [Inject] private IAuthenticationService _authenticationService;
        
        public override void OnShow()
        {
            addFriendInputField.text = "";
            addFriendResponseText.text = "";
            addFriendButton.enabled = false;
            
            ClearFriendRequests();
            ClearOutgoingFriendRequests();
            SpawnFriendRequests();
            
            _friendService.OnFriendRequestReceived += OnFriendRequestReceived;
            _friendService.OnFriendRequestRespondedTo += OnFriendRequestRespondedTo;
            _friendService.OnFriendRequestCanceled += OnFriendRequestCanceled;
            
            templateFriendRequestItem.gameObject.SetActive(false);
            templateOutgoingFriendRequestItem.gameObject.SetActive(false);
            
            base.OnShow();
        }

        public override void OnHide()
        {
            ClearFriendRequests();
            ClearOutgoingFriendRequests();
            _friendService.OnFriendRequestReceived -= OnFriendRequestReceived;
            _friendService.OnFriendRequestRespondedTo -= OnFriendRequestRespondedTo;
            _friendService.OnFriendRequestCanceled -= OnFriendRequestCanceled;
            
            base.OnHide();
        }

        public async void OnClick_AddFriend()
        {
            if (_talkingToServer)
                return;
            _talkingToServer = true;

            (bool success, string message, UserSimple toUser) = await _friendService.SendFriendRequest(addFriendInputField.text);
            
            addFriendResponseText.text = message;
            if (success)
            {
                SpawnOutgoingFriendRequestItem(toUser);
            }

            _talkingToServer = false;
        }

        public void OnValueChanged_AddFriendInputField(string str)
        {
            addFriendButton.enabled = !string.IsNullOrEmpty(str);
        }

        private void ClearFriendRequests()
        {
            if (_friendRequestItems == null)
            {
                _friendRequestItems = new List<FriendRequestItem>();
                return;
            }
            
            foreach (var item in _friendRequestItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _friendRequestItems.Clear();  
        }
        
        private void ClearOutgoingFriendRequests()
        {
            if (_outgoingFriendRequestItems == null)
            {
                _outgoingFriendRequestItems = new List<FriendRequestItem>();
                return;
            }
            
            foreach (var item in _outgoingFriendRequestItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _outgoingFriendRequestItems.Clear();  
        }

        private void SpawnFriendRequests()
        {
            foreach (UserSimple user in _friendService.FriendRequests)
            {
                SpawnFriendRequestItem(user);
            }

            foreach (UserSimple user in _friendService.OutgoingFriendRequests)
            {
                SpawnOutgoingFriendRequestItem(user);
            }
        }
        
        private void OnFriendRequestCanceled(UserSimple user)
        {
            FriendRequestItem item = _friendRequestItems.Find(item => item.User.UserID.Equals(user.UserID));
            if (item == null)
                return;
            
            Destroy(item.gameObject);
        }
        
        private void OnFriendRequestReceived(UserSimple user)
        {
            SpawnFriendRequestItem(user);
        }
        
        private void OnFriendRequestRespondedTo(FriendRequestRespondNotification notification)
        {
            FriendRequestItem item = _outgoingFriendRequestItems.Find(item => item.User.UserID.Equals(notification.ToUser.UserID));
            if (item == null)
                return;
            
            Destroy(item.gameObject);
        }

        private void SpawnFriendRequestItem(UserSimple user)
        {
            if (user == null)
                return;
            
            FriendRequestItem item = Instantiate(templateFriendRequestItem, templateFriendRequestItem.transform.parent);
            item.Setup(user, OnRespondToRequest);
            item.gameObject.SetActive(true);
            
            _friendRequestItems.Add(item);
        }

        private void SpawnOutgoingFriendRequestItem(UserSimple user)
        {
            if (user == null)
                return;
            
            FriendRequestItem item = Instantiate(templateOutgoingFriendRequestItem, templateOutgoingFriendRequestItem.transform.parent);
            item.Setup(user, OnCancelFriendRequest);
            item.gameObject.SetActive(true);
            
            _outgoingFriendRequestItems.Add(item);
        }

        private async void OnRespondToRequest(FriendRequestItem item, bool result)
        {
            UserSimple user = item.User;
            
            _friendRequestItems.Remove(item);
            Destroy(item.gameObject);

            (bool functionResult, string message) = await _friendService.RespondToFriendRequest(user.UserID, _authenticationService.CurrentUser.UserID, result, isCanceling: false);

            _friendService.FriendRequests.Remove(user);
            if (functionResult)
                _friendService.AddFriendToFriendsList(user);
        }

        private async void OnCancelFriendRequest(FriendRequestItem item, bool result)
        {
            UserSimple user = item.User;
            
            _friendRequestItems.Remove(item);
            Destroy(item.gameObject);
            
            (bool, string) res = await _friendService.RespondToFriendRequest(_authenticationService.CurrentUser.UserID, user.UserID, false, isCanceling: true);
            
            _friendService.OutgoingFriendRequests.Remove(user);
        } 
    }
}
