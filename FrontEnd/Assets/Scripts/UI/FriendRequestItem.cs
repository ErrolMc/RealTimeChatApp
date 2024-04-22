using System.Collections;
using System.Collections.Generic;
using ChatApp.Shared.Notifications;
using TMPro;
using UnityEngine;

public class FriendRequestItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;

    private FriendRequestNotification _notification;
    private bool _hasClicked = false;
    
    public async void OnClickResult(bool result)
    {
        if (_hasClicked)
            return;
        _hasClicked = true;
        
        
    }

    public void Setup(FriendRequestNotification notification)
    {
        _notification = notification;
        nameText.text = notification.FromUserName;
    }
}
