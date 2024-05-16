using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class MasterWindowViewModel : ViewModelBase
    {
        private PanelViewModelBase _currentPanel;
        public PanelViewModelBase CurrentPanel
        {
            get => _currentPanel; 
            set => this.RaiseAndSetIfChanged(ref _currentPanel, value);
        }
        
        public MasterWindowViewModel()
        {
            
        }
    }
}
