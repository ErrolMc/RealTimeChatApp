using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
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
        private IServiceProvider? _serviceProvider;
        
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ConfigureServices()
        {
            IServiceCollection collection = new ServiceCollection();

            collection.AddSingleton<IAuthenticationService, AuthenticationService>();
            collection.AddSingleton<INavigationService, NavigationService>();
            collection.AddSingleton<MasterWindowViewModel>(new MasterWindowViewModel());
            
            collection.AddTransient<LoginPanelViewModel>();
            collection.AddTransient<RegisterPanelViewModel>();
            collection.AddTransient<SettingsPanelViewModel>();
            collection.AddTransient<FriendsPanelViewModel>();
            collection.AddTransient<ChatViewModel>();
            collection.AddTransient<SidebarViewModel>();
            collection.AddTransient<SidebarBottomViewModel>();
            collection.AddTransient<MainPanelViewModel>();

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
    }
}