using System.Windows.Input;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class CreateGroupDMUserViewModel : ViewModelBase
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
        private readonly System.Action<CreateGroupDMUserViewModel> _callback;
        
        public CreateGroupDMUserViewModel(UserSimple user, System.Action<CreateGroupDMUserViewModel> callback)
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