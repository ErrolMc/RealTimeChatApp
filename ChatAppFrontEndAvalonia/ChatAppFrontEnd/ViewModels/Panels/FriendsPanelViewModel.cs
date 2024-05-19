using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatAppFrontEnd.Source.Services;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendsPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        
        private ObservableCollection<FriendRequestItemViewModel> _friendRequestItems;
        private ObservableCollection<FriendRequestItemViewModel> _outgoingFriendRequestItems;
        
        public ObservableCollection<FriendRequestItemViewModel> FriendRequestItems
        {
            get => _friendRequestItems;
            set => this.RaiseAndSetIfChanged(ref _friendRequestItems, value);
        } 
        
        public ObservableCollection<FriendRequestItemViewModel> OutgoingFriendRequestItems
        {
            get => _outgoingFriendRequestItems;
            set => this.RaiseAndSetIfChanged(ref _outgoingFriendRequestItems, value);
        } 
        
        public ICommand BackCommand { get; }

        public FriendsPanelViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            FriendRequestItems = new ObservableCollection<FriendRequestItemViewModel>()
            {
                new FriendRequestItemViewModel(),
                new FriendRequestItemViewModel(),
            };
            
            OutgoingFriendRequestItems = new ObservableCollection<FriendRequestItemViewModel>()
            {
                new FriendRequestItemViewModel(false),
                new FriendRequestItemViewModel(false),
            };
            
            BackCommand = ReactiveCommand.Create(GoBack);
        }

        private void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}

