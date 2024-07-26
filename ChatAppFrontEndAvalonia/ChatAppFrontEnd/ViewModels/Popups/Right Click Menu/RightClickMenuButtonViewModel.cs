using System;
using Avalonia;
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

        private readonly Action<Point> _onClickAction;
        
        public RightClickMenuButtonViewModel(string label, Action<Point> onClickAction)
        {
            _onClickAction = onClickAction;

            LabelText = label;
        }
        
        public void OnClick(Point clickPos)
        {
            _onClickAction?.Invoke(clickPos);
        }
    }
}

