using System.Windows.Input;
using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class CreateGroupDMSelectedUserViewModel : ViewModelBase
    {
        public UserSimple User { get; private set; }
        
        private string _nameText;
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        private readonly System.Action<CreateGroupDMSelectedUserViewModel> _callback;
        public ICommand OnClickCommand { get; }

        public CreateGroupDMSelectedUserViewModel(UserSimple user, System.Action<CreateGroupDMSelectedUserViewModel> callback)
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