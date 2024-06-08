using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ChatMessageViewModel : ViewModelBase
    {
        private string _bodyText;
        private string _nameText;
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }
        
        public string BodyText
        {
            get => _bodyText;
            set => this.RaiseAndSetIfChanged(ref _bodyText, value);
        }

        public ChatMessageViewModel(string nameText, string bodyText)
        {
            NameText = nameText;
            BodyText = bodyText;
        }
    }
}

