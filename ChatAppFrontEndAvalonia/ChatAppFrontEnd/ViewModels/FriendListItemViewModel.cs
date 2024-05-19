using System;
using System.Windows.Input;
using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class FriendListItemViewModel : ViewModelBase
    {
        public UserSimple User { get; private set; }
        public bool IsSelected { get; set; }
        
        public ICommand OnClickCommand { get; }
        
        private readonly Action<FriendListItemViewModel> _onClickAction;
        private string _nameText;
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        public FriendListItemViewModel(UserSimple user, Action<FriendListItemViewModel> onClickAction)
        {
            User = user;
            _onClickAction = onClickAction;
            
            NameText = user.UserName;

            OnClickCommand = ReactiveCommand.Create(OnClick);
        }
        
        private void OnClick()
        {
            _onClickAction?.Invoke(this);
        }
    }
}

