using UnityEngine;
using Zenject;
using ChatApp.Services;
using ChatApp.UI;

public class DIController : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind(typeof(IInitializable), typeof(IBroadcastService)).To<BroadcastService>().AsSingle().NonLazy();
    }
}