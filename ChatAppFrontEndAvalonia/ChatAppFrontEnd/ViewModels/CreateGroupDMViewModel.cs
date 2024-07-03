using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChatApp.Shared.Misc;
using ChatApp.Shared.Tables;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class CreateGroupDMViewModel : ViewModelBase
    {
        private const int MAX_PEOPLE_IN_GROUP = 10;
        
        private string _usernameField;
        public string UsernameField
        {
            get => _usernameField;
            set => this.RaiseAndSetIfChanged(ref _usernameField, value);
        }
        
        public string SubHeadingText => $"You can add {MAX_PEOPLE_IN_GROUP - _checkedUsers.Count - 1} more friends.";
        
        private ObservableCollection<CreateGroupDMSelectedUserViewModel> _selectedUsers;
        public ObservableCollection<CreateGroupDMSelectedUserViewModel> SelectedUsers
        {
            get => _selectedUsers;
            set => this.RaiseAndSetIfChanged(ref _selectedUsers, value);
        }
        
        private ObservableCollection<CreateGroupDMUserViewModel> _users;
        public ObservableCollection<CreateGroupDMUserViewModel> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }
        
        public ICommand CreateDMCommand { get; }

        private readonly List<UserSimple> _allFriends;
        private List<UserSimple> _checkedUsers;
        
        public CreateGroupDMViewModel(List<UserSimple> allFriends)
        {
            _allFriends = allFriends.OrderBy(friend => friend.UserName).ToList();
            _checkedUsers = new List<UserSimple>();
            
            SelectedUsers = new ObservableCollection<CreateGroupDMSelectedUserViewModel>();
            Users = new ObservableCollection<CreateGroupDMUserViewModel>();

            UsernameField = string.Empty;
            UpdateUserList();

            CreateDMCommand = ReactiveCommand.Create(OnClick_CreateDM);
        }

        public void UpdateUserList()
        {
            Users = new ObservableCollection<CreateGroupDMUserViewModel>();
            
            foreach (UserSimple friend in _allFriends)
            {
                if (!friend.UserName.StartsWith(UsernameField, false, null))
                    continue;
                
                Users.Add(new CreateGroupDMUserViewModel(friend, OnUserCheckChanged) { Checked = _checkedUsers.Contains(friend) });
            }
        }

        private async void OnClick_CreateDM()
        {
            // create group dm
        }

        private void OnUserCheckChanged(CreateGroupDMUserViewModel userViewModel)
        {
            UserSimple user = userViewModel.User;
            
            if (userViewModel.Checked) // removing check
            { 
                _checkedUsers.Remove(user);
                
                CreateGroupDMSelectedUserViewModel selectedUser = SelectedUsers.FirstOrDefault(u => u.User.UserID == user.UserID);
                if (selectedUser != null)
                    SelectedUsers.Remove(selectedUser);
            }
            else // checking
            { 
                if (_checkedUsers.Count >= MAX_PEOPLE_IN_GROUP - 1)
                    return;
                
                SelectedUsers.Add(new CreateGroupDMSelectedUserViewModel(user, OnRemoveSelectedUser));
                _checkedUsers.Add(user);
            }

            _checkedUsers = _checkedUsers.OrderBy(friend => friend.UserName).ToList();

            this.RaisePropertyChanged(nameof(SubHeadingText));
            UpdateUserList();
        }

        private void OnRemoveSelectedUser(CreateGroupDMSelectedUserViewModel selectedUserViewModel)
        {
            _checkedUsers.Remove(selectedUserViewModel.User);
            SelectedUsers.Remove(selectedUserViewModel);
            
            this.RaisePropertyChanged(nameof(SubHeadingText));
            UpdateUserList();
        }
    }
}
