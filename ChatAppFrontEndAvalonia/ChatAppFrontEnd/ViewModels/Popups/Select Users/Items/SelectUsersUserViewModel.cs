using System.Windows.Input;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class SelectUsersUserViewModel : ViewModelBase
    {
        public UserSimple User { get; private set; }
        
        private string _nameText;
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set => this.RaiseAndSetIfChanged(ref _checked, value);
        }

        public ICommand OnClickCommand { get; }
        private readonly System.Action<SelectUsersUserViewModel> _callback;
        
        public SelectUsersUserViewModel(UserSimple user, System.Action<SelectUsersUserViewModel> callback)
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