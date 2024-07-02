using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatApp.Shared.Misc;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class CreateGroupDMViewModel : ViewModelBase
    {
        private string _usernameField;
        private ObservableCollection<CreateGroupDMSelectedUserViewModel> _selectedUsers;
        private ObservableCollection<CreateGroupDMUserViewModel> _users;
        
        public string UsernameField
        {
            get => _usernameField;
            set => this.RaiseAndSetIfChanged(ref _usernameField, value);
        }
        
        public ObservableCollection<CreateGroupDMSelectedUserViewModel> SelectedUsers
        {
            get => _selectedUsers;
            set => this.RaiseAndSetIfChanged(ref _selectedUsers, value);
        }
        
        public ObservableCollection<CreateGroupDMUserViewModel> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }
        
        public ICommand CreateDMCommand { get; }

        public CreateGroupDMViewModel()
        {
            CreateDMCommand = ReactiveCommand.Create(OnClick_CreateDM);

            SelectedUsers = new ObservableCollection<CreateGroupDMSelectedUserViewModel>()
            {
                new CreateGroupDMSelectedUserViewModel(new UserSimple() { UserName = "Errol" }),
                new CreateGroupDMSelectedUserViewModel(new UserSimple() { UserName = "John" }),
                new CreateGroupDMSelectedUserViewModel(new UserSimple() { UserName = "William" }),
            };
            
            Users = new ObservableCollection<CreateGroupDMUserViewModel>()
            {
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "Errol" }),
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "John" }),
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "William" }),
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "Peter" }),
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "Mike" }),
                new CreateGroupDMUserViewModel(new UserSimple() { UserName = "Nick" }),
            };
        }

        public void OnUsernameTextChanged()
        {
            
        }

        private void OnClick_CreateDM()
        {
            
        }
    }
}
