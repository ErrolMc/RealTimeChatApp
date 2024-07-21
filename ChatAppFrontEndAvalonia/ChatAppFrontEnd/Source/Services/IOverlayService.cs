using System;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Source.Services
{
    public interface IOverlayService
    {
        void ShowOverlay(ViewModelBase viewModel, double topOffset, double leftOffset, Action clickOutsideAction);
        void ShowOverlayCentered(ViewModelBase viewModel, Action clickOutsideAction);
        void HideOverlay();
    }
}

