using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendsPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        
        public ICommand BackCommand { get; }

        public FriendsPanelViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            
            BackCommand = ReactiveCommand.Create(GoBack);
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}

