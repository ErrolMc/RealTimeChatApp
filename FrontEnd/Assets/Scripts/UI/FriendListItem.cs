using System.Collections;
using System.Collections.Generic;
using ChatApp.Shared.Misc;
using TMPro;
using UnityEngine;

namespace ChatApp.UI
{
    public class FriendListItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        private UserSimple _user;
        
        public void Setup(UserSimple user)
        {
            _user = user;
            nameText.text = _user.UserName;
        }

        public void OnClick_SelectUser()
        {
            Debug.Log("Select User");
        }
    }   
}
