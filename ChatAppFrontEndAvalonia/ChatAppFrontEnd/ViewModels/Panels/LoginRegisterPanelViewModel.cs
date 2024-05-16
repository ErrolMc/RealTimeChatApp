using ChatAppFrontEnd.Source.Services;
using ChatAppFrontEnd.ViewModels;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class LoginRegisterPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public LoginRegisterPanelViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _currentViewModel = new LoginViewModel(ShowRegister, GotoChat);
        }

        private void ShowLogin()
        {
            CurrentViewModel = new LoginViewModel(ShowRegister, GotoChat);
        }

        private void ShowRegister()
        {
            CurrentViewModel = new RegisterViewModel(ShowLogin);
        }

        private void GotoChat()
        {
            _navigationService.Navigate<MainPanelViewModel>();
        }
    }   
}