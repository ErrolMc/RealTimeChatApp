using System;
using System.Windows.Input;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class ConfirmRemoveDialogViewModel : ViewModelBase
    {
        private string _headerText;
        public string HeaderText
        {
            get => _headerText;
            set => this.RaiseAndSetIfChanged(ref _headerText, value);
        }
        
        private string _bodyText;
        public string BodyText
        {
            get => _bodyText;
            set => this.RaiseAndSetIfChanged(ref _bodyText, value);
        }
        
        private string _confirmButtonText;
        public string ConfirmButtonText
        {
            get => _confirmButtonText;
            set => this.RaiseAndSetIfChanged(ref _confirmButtonText, value);
        }
        
        public ICommand CancelCommand { get; }
        public ICommand ConfirmCommand { get; }

        private readonly Action<bool> _onClickAction;
        
        public ConfirmRemoveDialogViewModel(string headerText, string bodyText, string confirmButtonText, Action<bool> onClickAction)
        {
            HeaderText = headerText;
            BodyText = bodyText;
            ConfirmButtonText = confirmButtonText;

            _onClickAction = onClickAction;

            CancelCommand = ReactiveCommand.Create(OnClick_Cancel);
            ConfirmCommand = ReactiveCommand.Create(OnClick_Confirm);
        }

        private void OnClick_Cancel()
        {
            _onClickAction?.Invoke(false);
        }
        
        private void OnClick_Confirm()
        {
            _onClickAction?.Invoke(true);
        }
    }
}

