using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Source.Services
{
    public interface INavigationService
    {
        void Navigate<T>(bool clearBackStack = false) where T : PanelViewModelBase;
        void GoBack();
        void ClearBackStack();
        bool CanGoBack { get; }
        PanelViewModelBase CurrentPanel { get; }
    }   
}