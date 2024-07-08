using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ChatApp.Services;
using ChatApp.Source.Services;
using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.Source.Services.Concrete;
using ChatAppFrontEnd.ViewModels;
using ChatAppFrontEnd.Views;
using ChatAppFrontEnd.Views.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAppFrontEnd
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ConfigureServices()
        {
            IServiceCollection collection = new ServiceCollection();
            
            // register services
            collection.AddSingleton<IAuthenticationService, AuthenticationService>();
            collection.AddSingleton<IChatService, ChatService>();
            collection.AddSingleton<IFriendService, FriendService>();
            collection.AddSingleton<IGroupService, GroupService>();
            collection.AddSingleton<INavigationService, NavigationService>();
            collection.AddSingleton<ISignalRService, SignalRService>();
            collection.AddSingleton<IOverlayService, OverlayService>();
            
            // using lazy to get around circular dependency (SignalRService -> NotificationService -> ChatService -> SignalRService)
            collection.AddTransient<INotificationService, NotificationService>(provider =>
            {
                var friendService = new Lazy<IFriendService>(provider.GetRequiredService<IFriendService>);
                var chatService = new Lazy<IChatService>(provider.GetRequiredService<IChatService>);
                var groupService = new Lazy<IGroupService>(provider.GetRequiredService<IGroupService>);
                return new NotificationService(friendService, chatService, groupService);
            });
            
            // register viewmodels
            collection.AddSingleton<MasterWindowViewModel>(new MasterWindowViewModel());
            
            collection.AddTransient<LoginPanelViewModel>();
            collection.AddTransient<RegisterPanelViewModel>();
            collection.AddTransient<SettingsPanelViewModel>();
            collection.AddTransient<FriendsPanelViewModel>();
            collection.AddTransient<ChatViewModel>();
            collection.AddTransient<DMSidebarViewModel>();
            collection.AddTransient<SidebarBottomViewModel>();
            collection.AddTransient<ServerSidebarViewModel>();
            collection.AddTransient<ChatHistoryViewModel>();
            
            collection.AddSingleton<MainPanelViewModel>();
            
            _serviceProvider = collection.BuildServiceProvider();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            ConfigureServices();
            
            if (_serviceProvider == null)
            {
                base.OnFrameworkInitializationCompleted();
                return;
            }
            
            var masterWindowViewModel = _serviceProvider.GetRequiredService<MasterWindowViewModel>();
        
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MasterWindowView()
                {
                    DataContext = masterWindowViewModel
                };
                
                desktop.Exit += OnApplicationExit;
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MasterWindowView()
                {
                    DataContext = masterWindowViewModel
                };
            }
            
            var navigationService = _serviceProvider.GetRequiredService<INavigationService>();
            navigationService.Navigate<LoginPanelViewModel>();
            
            base.OnFrameworkInitializationCompleted();
        }
        
        private void OnApplicationExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            var signalRService = _serviceProvider.GetService<ISignalRService>();
            signalRService?.OnApplicationQuit();
        }
    }
}