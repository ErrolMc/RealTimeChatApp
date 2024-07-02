using System;
using System.Windows.Input;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MasterWindowViewModel : ViewModelBase
    {
        private PanelViewModelBase _currentPanel;
        public PanelViewModelBase CurrentPanel
        {
            get => _currentPanel; 
            set => this.RaiseAndSetIfChanged(ref _currentPanel, value);
        }
        
        private ViewModelBase _overlayViewModel;
        public ViewModelBase OverlayViewModel
        {
            get => _overlayViewModel; 
            set => this.RaiseAndSetIfChanged(ref _overlayViewModel, value);
        }

        private bool _isOverlayVisible;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible; 
            set => this.RaiseAndSetIfChanged(ref _isOverlayVisible, value);
        }

        private double _overlayOffsetLeft;
        public double OverlayOffsetLeft
        {
            get => _overlayOffsetLeft;
            set => this.RaiseAndSetIfChanged(ref _overlayOffsetLeft, value);
        }
        
        private double _overlayOffsetTop;
        public double OverlayOffsetTop
        {
            get => _overlayOffsetTop;
            set => this.RaiseAndSetIfChanged(ref _overlayOffsetTop, value);
        }

        public Action OnClickOutsideOverlay { get; set; }
        public ICommand ClickOutsideOverlayCommand { get; }
        
        public MasterWindowViewModel()
        {
            ClickOutsideOverlayCommand = ReactiveCommand.Create(OnClick_OutsideOverlay);
        }

        private void OnClick_OutsideOverlay()
        {
            OnClickOutsideOverlay?.Invoke();
        }
    }
}
