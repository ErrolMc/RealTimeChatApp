using System.Collections;
using System.Collections.Generic;
using ChatApp.Shared.Notifications;
using TMPro;
using UnityEngine;

namespace ChatApp.UI
{
    public class FriendRequestItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        public FriendRequestNotification Notification { get; set; }
    
        private bool _hasClicked = false;
        private System.Action<FriendRequestItem, bool> _callback;
    
        public void OnClickResult(bool result)
        {
            if (_hasClicked)
                return;
            _hasClicked = true;
        
            _callback.Invoke(this, result);
        }

        public void Setup(FriendRequestNotification notification, System.Action<FriendRequestItem, bool> callback)
        {
            _callback = callback;
            Notification = notification;
            nameText.text = notification.FromUser.UserName;
        }
    }
}
