using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatTopBarViewModel : ViewModelBase
    {
        private bool _isShown;
        private string _userName;

        public bool IsShown
        {
            get => _isShown;
            set => this.RaiseAndSetIfChanged(ref _isShown, value);
        }

        public string UserName
        {
            get => _userName;
            set => this.RaiseAndSetIfChanged(ref _userName, value);
        }
        
        public ChatTopBarViewModel()
        {
            IsShown = false;
        }

        public void Setup(UserSimple user)
        {
            UserName = user.UserName;
            IsShown = true;
        }
    }
}