using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using ChatApp.Shared.Notifications;
using ChatApp.UI;
using TMPro;
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

        private bool _talkingToServer = false;
        private List<FriendRequestItem> _friendRequestItems;

        [Inject] private IFriendService _friendService;
        
        public override void OnShow()
        {
            addFriendInputField.text = "";
            addFriendResponseText.text = "";
            addFriendButton.enabled = false;
            
            ClearFriendRequests();
            SpawnFriendRequests();
            _friendService.OnFriendRequestReceived += SpawnFriendRequestItem;
            templateFriendRequestItem.gameObject.SetActive(false);
            
            base.OnShow();
        }

        public override void OnHide()
        {
            ClearFriendRequests();
            _friendService.OnFriendRequestReceived -= SpawnFriendRequestItem;
            
            base.OnHide();
        }

        public async void OnClick_AddFriend()
        {
            if (_talkingToServer)
                return;
            _talkingToServer = true;

            (bool, string) result = await _friendService.AddFriend(addFriendInputField.text);
            
            addFriendResponseText.text = result.Item2;

            _talkingToServer = false;
        }

        public void OnValueChanged_AddFriendInputField(string str)
        {
            addFriendButton.enabled = !string.IsNullOrEmpty(str);
        }

        private void ClearFriendRequests()
        {
            if (_friendRequestItems != null)
            {
                for (int i = 0; i < _friendRequestItems.Count; i++)
                    Destroy(_friendRequestItems[i].gameObject);
                _friendRequestItems.Clear();
            }
            else
                _friendRequestItems = new List<FriendRequestItem>();
        }

        private void SpawnFriendRequests()
        {
            foreach (FriendRequestNotification notification in _friendService.ReceivedFriendRequestsThisSession)
            {
                SpawnFriendRequestItem(notification);
            }
        }

        private void SpawnFriendRequestItem(FriendRequestNotification notification)
        {
            FriendRequestItem item = Instantiate(templateFriendRequestItem, templateFriendRequestItem.transform.parent);
            item.Setup(notification);
            item.gameObject.SetActive(true);
            
            _friendRequestItems.Add(item);
        }
    }
}
