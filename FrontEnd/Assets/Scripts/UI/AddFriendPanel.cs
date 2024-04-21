using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
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

        private bool _talkingToServer = false;

        [Inject] private IFriendService _friendService;
        
        public override void OnShow()
        {
            addFriendInputField.text = "";
            addFriendResponseText.text = "";
            addFriendButton.enabled = false;
            
            base.OnShow();
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
    }
}
