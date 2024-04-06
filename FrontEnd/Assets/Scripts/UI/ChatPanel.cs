using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;
using ChatApp.Services;

namespace ChatApp.UI
{
    public class ChatPanel : Panel
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private ChatMessage templateMessage;

        [Inject] private IBroadcastService _broadcastService;

        public override void OnShow()
        {
            _broadcastService.OnMessageReceived += ReceiveMessage;
            templateMessage.gameObject.SetActive(false);
            
            base.OnShow();
        }

        public override void OnHide()
        {
            base.OnHide();
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
