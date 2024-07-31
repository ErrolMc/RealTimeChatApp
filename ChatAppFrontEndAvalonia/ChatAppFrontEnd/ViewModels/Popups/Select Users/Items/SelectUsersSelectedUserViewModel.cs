using System.Windows.Input;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class SelectUsersSelectedUserViewModel : ViewModelBase
    {
        public UserSimple User { get; private set; }
        
        private string _nameText;
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        private readonly System.Action<SelectUsersSelectedUserViewModel> _callback;
        public ICommand OnClickCommand { get; }

        public SelectUsersSelectedUserViewModel(UserSimple user, System.Action<SelectUsersSelectedUserViewModel> callback)
        {
            User = user;
            _callback = callback;
            
            NameText = User.UserName;
            
            OnClickCommand = ReactiveCommand.Create(OnClick);
        }
        
        private void OnClick()
        {
            _callback?.Invoke(this);
        }
    }   
}