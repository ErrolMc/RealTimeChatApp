using System.Collections.ObjectModel;
using System.Windows.Input;
using ChatAppFrontEnd.Views;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class CreateGroupDMViewModel : ViewModelBase
    {
        private string _usernameField;
        private ObservableCollection<CreateGroupDMSelectedUserView> _selectedUsers;
        private ObservableCollection<CreateGroupDMUserView> _users;
        
        public string UsernameField
        {
            get => _usernameField;
            set => this.RaiseAndSetIfChanged(ref _usernameField, value);
        }
        
        public ObservableCollection<CreateGroupDMSelectedUserView> SelectedUsers
        {
            get => _selectedUsers;
            set => this.RaiseAndSetIfChanged(ref _selectedUsers, value);
        }
        
        public ObservableCollection<CreateGroupDMUserView> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }
        
        public ICommand CreateDMCommand { get; }

        public CreateGroupDMViewModel()
        {
            CreateDMCommand = ReactiveCommand.Create(OnClick_CreateDM);
        }

        public void OnUsernameTextChanged()
        {
            
        }

        private void OnClick_CreateDM()
        {
            
        }
        
    }
}

