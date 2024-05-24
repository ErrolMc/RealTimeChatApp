using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChatApp.Services;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Notifications;
using ChatAppFrontEnd.Source.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendsPanelViewModel : PanelViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IFriendService _friendService;
        private readonly IAuthenticationService _authenticationService;
        
        private bool _talkingToServer = false;

        private string friendNameInputValue;
        private string addFriendResponseValue;
        private ObservableCollection<FriendRequestItemViewModel> _friendRequestItems;
        private ObservableCollection<FriendRequestItemViewModel> _outgoingFriendRequestItems;
        
        public string FriendNameInputValue
        {
            get => friendNameInputValue;
            set => this.RaiseAndSetIfChanged(ref friendNameInputValue, value);
        }
        
        public string AddFriendResponseValue
        {
            get => addFriendResponseValue;
            set => this.RaiseAndSetIfChanged(ref addFriendResponseValue, value);
        }
        
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
        public ICommand AddFriendCommand { get; }

        public override void OnShow()
        {
            _friendService.OnFriendRequestReceived += OnFriendRequestReceived;
            _friendService.OnFriendRequestRespondedTo += OnFriendRequestRespondedTo;
            _friendService.OnFriendRequestCanceled += OnFriendRequestCanceled;
            
            AddFriendResponseValue = string.Empty;

            FriendRequestItems = new ObservableCollection<FriendRequestItemViewModel>();
            foreach (var request in _friendService.FriendRequests)
                SpawnFriendRequestItem(request);

            OutgoingFriendRequestItems = new ObservableCollection<FriendRequestItemViewModel>();
            foreach (var request in _friendService.OutgoingFriendRequests)
                SpawnOutgoingFriendRequestItem(request);
        }

        public FriendsPanelViewModel(INavigationService navigationService, IFriendService friendService, IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _friendService = friendService;
            _authenticationService = authenticationService;

            BackCommand = ReactiveCommand.Create(GoBack);
            AddFriendCommand = ReactiveCommand.Create(AddFriend);
        }

        private void GoBack()
        {
            _friendService.OnFriendRequestReceived += OnFriendRequestReceived;
            _friendService.OnFriendRequestRespondedTo += OnFriendRequestRespondedTo;
            _friendService.OnFriendRequestCanceled += OnFriendRequestCanceled;
            
            _navigationService.GoBack();
        }

        private async void AddFriend()
        {
            if (_talkingToServer)
                return;
            _talkingToServer = true;

            (bool success, string message, UserSimple toUser) = await _friendService.SendFriendRequest(FriendNameInputValue);
            
            AddFriendResponseValue = message;
            if (success)
            {
                SpawnOutgoingFriendRequestItem(toUser);
            }

            _talkingToServer = false;
        }
        
        #region private
        private void SpawnFriendRequestItem(UserSimple user)
        {
            if (user == null)
                return;

            FriendRequestItemViewModel item = new FriendRequestItemViewModel(user, OnRespondToRequest, true);
            FriendRequestItems.Add(item);
        }

        private void SpawnOutgoingFriendRequestItem(UserSimple user)
        {
            if (user == null)
                return;

            FriendRequestItemViewModel item = new FriendRequestItemViewModel(user, OnCancelFriendRequest, false);
            OutgoingFriendRequestItems.Add(item);
        }
        
        private async void OnRespondToRequest(FriendRequestItemViewModel item, bool result)
        {
            UserSimple user = item.User;

            FriendRequestItems.Remove(item);

            (bool functionResult, string message) = await _friendService.RespondToFriendRequest(user.UserID, _authenticationService.CurrentUser.UserID, result, isCanceling: false);

            _friendService.FriendRequests.Remove(user);
            if (functionResult)
                _friendService.AddFriendToLocalFriendsList(user);
        }
        
        private async void OnCancelFriendRequest(FriendRequestItemViewModel item, bool result)
        {
            UserSimple user = item.User;
            
            OutgoingFriendRequestItems.Remove(item);
            
            (bool, string) res = await _friendService.RespondToFriendRequest(_authenticationService.CurrentUser.UserID, user.UserID, false, isCanceling: true);
            
            _friendService.OutgoingFriendRequests.Remove(user);
        } 
        #endregion
        
        #region events from friendservice
        private void OnFriendRequestCanceled(UserSimple user)
        {
            var item = FriendRequestItems.SingleOrDefault(item => item.User.UserID.Equals(user.UserID));
            if (item == null)
                return;
            
            FriendRequestItems.Remove(item);
        }
        
        private void OnFriendRequestReceived(UserSimple user)
        {
            SpawnFriendRequestItem(user);
        }
        
        private void OnFriendRequestRespondedTo(FriendRequestRespondNotification notification)
        {
            var item = OutgoingFriendRequestItems.SingleOrDefault(item => item.User.UserID.Equals(notification.ToUser.UserID));
            if (item == null)
                return;
            
            OutgoingFriendRequestItems.Remove(item);
        }
        #endregion
    }
}

