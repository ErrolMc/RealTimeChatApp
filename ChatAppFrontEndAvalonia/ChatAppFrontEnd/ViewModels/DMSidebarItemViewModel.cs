using System;
using System.Windows.Input;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class DMSidebarItemViewModel : ViewModelBase
    {
        public IChatEntity ChatEntity { get; private set; }
        public bool IsSelected { get; set; }
        
        public ICommand OnClickCommand { get; }
        
        private readonly Action<DMSidebarItemViewModel> _onClickAction;
        private string _nameText;
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        public DMSidebarItemViewModel(IChatEntity chatEntity, Action<DMSidebarItemViewModel> onClickAction)
        {
            ChatEntity = chatEntity;
            _onClickAction = onClickAction;
            
            NameText = ChatEntity.Name;

            OnClickCommand = ReactiveCommand.Create(OnClick);
        }
        
        private void OnClick()
        {
            _onClickAction?.Invoke(this);
        }
    }
}

