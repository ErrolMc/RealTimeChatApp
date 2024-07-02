using System.Windows.Input;
using ChatApp.Shared.Misc;
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
        
        public CreateGroupDMUserViewModel(UserSimple user)
        {
            User = user;

            NameText = User.UserName;
            
            OnClickCommand = ReactiveCommand.Create(OnClick);
        }

        private void OnClick()
        {
            
        }
    }   
}