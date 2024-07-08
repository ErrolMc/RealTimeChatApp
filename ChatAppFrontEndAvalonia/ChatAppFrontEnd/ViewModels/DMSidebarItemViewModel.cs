using System;
using System.Windows.Input;
using ChatApp.Shared.GroupDMs;
using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class DMSidebarItemViewModel : ViewModelBase
    {
        public bool IsGroupDM { get; private set; }
        public UserSimple User { get; private set; }
        public GroupDMSimple GroupDM { get; private set; }
        
        public bool IsSelected { get; set; }
        
        public ICommand OnClickCommand { get; }
        
        private readonly Action<DMSidebarItemViewModel> _onClickAction;
        private string _nameText;
        
        public string NameText
        {
            get => _nameText;
            set => this.RaiseAndSetIfChanged(ref _nameText, value);
        }

        public DMSidebarItemViewModel(UserSimple user, Action<DMSidebarItemViewModel> onClickAction)
        {
            IsGroupDM = false;
            
            User = user;
            _onClickAction = onClickAction;
            
            NameText = user.UserName;

            OnClickCommand = ReactiveCommand.Create(OnClick);
        }
        
        public DMSidebarItemViewModel(GroupDMSimple groupDM, Action<DMSidebarItemViewModel> onClickAction)
        {
            _onClickAction = onClickAction;

            IsGroupDM = true;
            GroupDM = groupDM;
            
            NameText = groupDM.Name;

            OnClickCommand = ReactiveCommand.Create(OnClick);
        }
        
        private void OnClick()
        {
            _onClickAction?.Invoke(this);
        }
    }
}

