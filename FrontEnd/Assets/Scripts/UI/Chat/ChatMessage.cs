using System.Collections;
using System.Collections.Generic;
using ChatApp.Shared.Misc;
using UnityEngine;
using TMPro;

namespace ChatApp.UI
{
    public class ChatMessage : MonoBehaviour
    {
        [SerializeField] private UserDisplayItem userDisplay;
        [SerializeField] private TextMeshProUGUI messageText;

        public void Setup(UserSimple fromUser, string message)
        {
            userDisplay.Setup(fromUser);
            messageText.text = message;
        }
    }   
}
