using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ChatApp.UI
{
    public class ChatMessage : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;

        public void SetMessageText(string message)
        {
            messageText.text = message;
        }
    }   
}
