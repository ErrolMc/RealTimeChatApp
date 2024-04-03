using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;

namespace ChatApp.UI
{
    public class ChatWindow : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ChatMessage templateMessage;

        [Inject] private IBroadcastService _broadcastService;

        void Start()
        {
            _broadcastService.OnMessageReceived += ReceiveMessage;
            templateMessage.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                string msg = inputField.text;
                if (!string.IsNullOrEmpty(msg))
                {
                    CreateMessage(msg);
                    _broadcastService.BroadcastMessage(msg);
                }
            }
        }

        public void ReceiveMessage(string message)
        {
            CreateMessage(message);
        }

        public void CreateMessage(string message)
        {
            ChatMessage chatMessage = Instantiate(templateMessage, templateMessage.transform.parent);
            chatMessage.SetMessageText(message);
            chatMessage.gameObject.SetActive(true);
        }
    }   
}
