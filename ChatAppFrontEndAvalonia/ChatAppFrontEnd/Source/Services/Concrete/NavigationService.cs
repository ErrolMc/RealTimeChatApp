using System;
using System.Collections.Generic;
using ChatAppFrontEnd.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class NavigationService : INavigationService
    {
        private readonly Stack<PanelViewModelBase> _navigationStack;
        private readonly IServiceProvider _serviceProvider;
        private readonly MasterWindowViewModel _masterWindowViewModel;
        
        public PanelViewModelBase CurrentPanel => _navigationStack.Peek();
        public bool CanGoBack => _navigationStack.Count > 1;
        
        public NavigationService(IServiceProvider serviceProvider, MasterWindowViewModel masterWindowViewModel)
        {
            _serviceProvider = serviceProvider;
            _masterWindowViewModel = masterWindowViewModel;
            _navigationStack = new Stack<PanelViewModelBase>();
        }

        public void Navigate<TPageViewModel>(bool clearBackStack = false) where TPageViewModel : PanelViewModelBase
        {
            if (_navigationStack.Count > 0)
                _navigationStack.Peek().OnHide();
            
            try
            {
                var viewModel = _serviceProvider.GetRequiredService<TPageViewModel>();
            
                if (clearBackStack)
                    ClearBackStack();
                else
                    _navigationStack.Push(viewModel);
            
                GotoCurrentWindow();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Service of type {nameof(TPageViewModel)} doesnt exist!");
            }
        }

        public void GoBack()
        {
            if (!CanGoBack) return;
            
            var prev = _navigationStack.Pop();
            prev.OnHide();
            GotoCurrentWindow();
        }

        public void ClearBackStack()
        {
            if (!CanGoBack) return;
            
            var current = _navigationStack.Pop();
            _navigationStack.Clear();
            _navigationStack.Push(current);
        }

        private void GotoCurrentWindow()
        {
            CurrentPanel.OnShow();
            _masterWindowViewModel.CurrentPanel = CurrentPanel;
        }
    }
}