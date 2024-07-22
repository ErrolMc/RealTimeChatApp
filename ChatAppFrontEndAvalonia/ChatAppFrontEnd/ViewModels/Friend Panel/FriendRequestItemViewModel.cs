using System.Windows.Input;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendRequestItemViewModel : ViewModelBase
    {
        private System.Action<FriendRequestItemViewModel, bool> _onClickAction;
        private bool _isIncoming = true;
        private string _nameText;
     
        public UserSimple User { get; set; }
        public bool IsIncoming
        {
            get => _isIncoming;
            set => this.RaiseAndSetIfChanged(ref _isIncoming, value);
        }
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }
        
        public ICommand YesCommand { get; }
        public ICommand NoCommand { get; }

        public FriendRequestItemViewModel(UserSimple user, System.Action<FriendRequestItemViewModel, bool> onClickAction, bool isIncoming = true)
        {
            IsIncoming = isIncoming;
            User = user;

            NameText = user.UserName;

            _onClickAction = onClickAction;
            
            YesCommand = ReactiveCommand.Create(OnClickYes);
            NoCommand = ReactiveCommand.Create(OnClickNo);
        }

        private void OnClickYes()
        {
            _onClickAction.Invoke(this, true);
        }
        
        private void OnClickNo()
        {
            _onClickAction.Invoke(this, false);
        }
    }
}