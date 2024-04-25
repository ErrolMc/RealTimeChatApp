using System;
using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace ChatApp.UI
{
    public class FriendsListUI : MonoBehaviour
    {
        [SerializeField] private FriendListItem templateFriendListItem;
        
        [Inject] private IFriendService _friendService;

        private List<FriendListItem> _friendListItems;
        private bool _isLiveUpdating = false;

        private void Awake()
        {
            templateFriendListItem.gameObject.SetActive(false);
        }

        public void OnShow()
        {
            if (_isLiveUpdating == true)
                return;
            
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
            FriendListItem item = Instantiate(templateFriendListItem, templateFriendListItem.transform.parent);
            item.Setup(friend);
            _friendListItems.Add(item);
        }

        private void OnFriendRequestRespondedTo(FriendRequestRespondNotification notification)
        {
            if (_isLiveUpdating && notification.Status)
                CreateFriendListItem(notification.ToUser);
        }
        
        private void ClearItems()
        {
            if (_friendListItems != null)
            {
                for (int i = 0; i < _friendListItems.Count; i++)
                    Destroy(_friendListItems[i].gameObject);
            }
            else
                _friendListItems = new List<FriendListItem>();
        }
    }   
}
