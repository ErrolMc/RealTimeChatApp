using System;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class RightClickMenuButtonViewModel : ViewModelBase
    {
        private string _labelText;
        public string LabelText
        {
            get => _labelText;
            set => this.RaiseAndSetIfChanged(ref _labelText, value);
        }

        private readonly Action _onClickAction;
        
        public RightClickMenuButtonViewModel(string label, Action onClickAction)
        {
            _onClickAction = onClickAction;

            LabelText = label;
        }
        
        public void OnClick()
        {
            _onClickAction?.Invoke();
        }
    }
}

