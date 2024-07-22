using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatTopBarViewModel : ViewModelBase
    {
        private bool _isShown;
        private string _displayName;

        public bool IsShown
        {
            get => _isShown;
            set => this.RaiseAndSetIfChanged(ref _isShown, value);
        }

        public string DisplayName
        {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
        
        public ChatTopBarViewModel()
        {
            IsShown = false;
        }

        public void Setup(IChatEntity chatEntity)
        {
            DisplayName = chatEntity.Name;
            IsShown = true;
        }
    }
}