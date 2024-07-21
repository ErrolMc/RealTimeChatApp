using System;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Source.Services.Concrete
{
    public class OverlayService : IOverlayService
    {
        private readonly MasterWindowViewModel _masterWindowViewModel;
        private Action _clickOutsideAction;
        
        public OverlayService(MasterWindowViewModel masterWindowViewModel)
        {
            _masterWindowViewModel = masterWindowViewModel;

            _masterWindowViewModel.OnClickOutsideOverlay += OnClickOutsideOverlay;
            HideOverlay();
        }

        private void OnClickOutsideOverlay()
        {
            _clickOutsideAction?.Invoke();
            HideOverlay();
        }

        public void ShowOverlay(ViewModelBase viewModel, double topOffset, double leftOffset, Action clickOutsideAction)
        {
            _clickOutsideAction = clickOutsideAction;
            
            _masterWindowViewModel.OverlayViewModelCenter = null;
            _masterWindowViewModel.OverlayViewModel = viewModel;
            _masterWindowViewModel.OverlayOffsetTop = topOffset;
            _masterWindowViewModel.OverlayOffsetLeft = leftOffset;
            _masterWindowViewModel.IsOverlayVisible = true;
            _masterWindowViewModel.IsOverlayCenterVisible = false;
        }
        
        public void ShowOverlayCentered(ViewModelBase viewModel, Action clickOutsideAction)
        {
            _clickOutsideAction = clickOutsideAction;
            
            _masterWindowViewModel.OverlayViewModel = null;
            _masterWindowViewModel.OverlayViewModelCenter = viewModel;
            _masterWindowViewModel.IsOverlayVisible = false;
            _masterWindowViewModel.IsOverlayCenterVisible = true;
        }

        public void HideOverlay()
        {
            _masterWindowViewModel.OverlayViewModelCenter = null;
            _masterWindowViewModel.OverlayViewModel = null;
            _masterWindowViewModel.IsOverlayVisible = false;
            _masterWindowViewModel.IsOverlayCenterVisible = false;
        }
    }
}

