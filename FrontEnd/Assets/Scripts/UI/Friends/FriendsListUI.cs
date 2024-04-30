using System;
using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace ChatApp.UI
{
    public class FriendsListUI : MonoBehaviour
    {
        [FormerlySerializedAs("templateFriendListItem")] [SerializeField] private UserDisplayItem templateUserDisplayItem;
        
        [Inject] private IFriendService _friendService;

        private System.Action<UserSimple> _onSelectUser;
        private List<UserDisplayItem> _friendListItems;
        private bool _isLiveUpdating = false;

        private void Awake()
        {
            templateUserDisplayItem.gameObject.SetActive(false);
        }

        public void OnShow(System.Action<UserSimple> onSelectUser)
        {
            if (_isLiveUpdating == true)
                return;

            _onSelectUser = onSelectUser;
            _friendService.OnFriendRequestRespondedTo += OnFriendRequestRespondedTo;
            _isLiveUpdating = true;
        }

        public void OnHide()
        {
            if (_isLiveUpdating == false)
                return;
            
            _friendService.OnFriendRequestRespondedTo -= OnFriendRequestRespondedTo;
            _isLiveUpdating = false;
            
            ClearItems();
        }

        public async UniTask<bool> PopulateFriendsList(bool callServer = false)
        {
            ClearItems();
            
            if (callServer)
            {
                bool res = await _friendService.UpdateFriendsList();

                if (!res)
                {
                    Debug.LogError("FriendListUI - Cant populate friends list");
                    return false;
                }   
            }

            List<UserSimple> friends = _friendService.Friends;
            Debug.Log($"FriendsListUI: Populating {friends.Count} friends");
            
            foreach (UserSimple friend in friends)
            {
                CreateFriendListItem(friend);
            }
            
            return true;
        }

        private void CreateFriendListItem(UserSimple friend)
        {
            UserDisplayItem item = Instantiate(templateUserDisplayItem, templateUserDisplayItem.transform.parent);
            item.Setup(friend);
            item.OnSelectUser += OnSelectItem;
            item.gameObject.SetActive(true);
            
            _friendListItems.Add(item);
        }

        private void OnSelectItem(UserSimple user)
        {
            _onSelectUser?.Invoke(user);
        }

        private void OnFriendRequestRespondedTo(FriendRequestRespondNotification notification)
        {
            if (_isLiveUpdating && notification.Status)
                CreateFriendListItem(notification.ToUser);
        }
        
        private void ClearItems()
        {
            if (_friendListItems == null)
            {
                _friendListItems = new List<UserDisplayItem>();
                return;
            }

            foreach (var item in _friendListItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _friendListItems.Clear(); 
        }
    }   
}
