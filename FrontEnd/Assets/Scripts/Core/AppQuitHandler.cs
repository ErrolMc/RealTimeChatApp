using System;
using System.Collections;
using System.Collections.Generic;
using ChatApp.Services;
using UnityEngine;
using Zenject;

namespace ChatApp.Core
{
    public class AppQuitHandler : MonoBehaviour
    {
        [Inject] private INotificationService _notificationService;

        public void OnApplicationQuit()
        {
            _notificationService.OnApplicationQuit();
        }
    }   
}
