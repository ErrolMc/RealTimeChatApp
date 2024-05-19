using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendRequestItemViewModel : ViewModelBase
    {
        private bool _isIncoming = true;
        
        public bool IsIncoming
        {
            get => _isIncoming;
            set => this.RaiseAndSetIfChanged(ref _isIncoming, value);
        }

        public FriendRequestItemViewModel(bool isIncoming = true)
        {
            IsIncoming = isIncoming;
        }
    }
}