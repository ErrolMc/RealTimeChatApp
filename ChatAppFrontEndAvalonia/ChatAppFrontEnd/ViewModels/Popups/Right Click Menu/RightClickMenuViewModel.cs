using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class RightClickMenuViewModel : ViewModelBase
    {
        private ObservableCollection<RightClickMenuButtonViewModel> _buttons;
        public ObservableCollection<RightClickMenuButtonViewModel> Buttons
        {
            get => _buttons;
            set => this.RaiseAndSetIfChanged(ref _buttons, value);
        }

        public RightClickMenuViewModel(List<RightClickMenuButtonViewModel> buttons)
        {
            Buttons = new ObservableCollection<RightClickMenuButtonViewModel>();
            foreach (RightClickMenuButtonViewModel button in buttons)
                Buttons.Add(button);
        }
    }
}
