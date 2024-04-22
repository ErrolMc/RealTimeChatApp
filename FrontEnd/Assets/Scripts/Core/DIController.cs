using System;
using System.Security.Cryptography;
using UnityEngine;
using Zenject;
using ChatApp.Services.Concrete;
using ChatApp.Services;
using ChatApp.UI;

public class DIController : MonoInstaller
{
    [SerializeField] private PanelManagementService panelManager;
    
    public override void InstallBindings()
    {
        Container.Bind(typeof(IInitializable), typeof(IPanelManagementService)).To<PanelManagementService>().FromInstance(panelManager).AsSingle().NonLazy();
        Container.Bind<IAuthenticationService>().To<AuthenticationService>().AsSingle().NonLazy();
        Container.Bind<INotificationService>().To<NotificationService>().AsSingle().NonLazy();
        Container.Bind(typeof(IInitializable), typeof(IFriendService)).To<FriendService>().AsSingle().NonLazy();
    }
}