using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
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
                
        private string _bottomMessageField;
        public string BottomMessageField
        {
            get => _bottomMessageField;
            set => this.RaiseAndSetIfChanged(ref _bottomMessageField, value);
        }
        
        private string _createButtonText;
        public string CreateButtonText
        {
            get => _createButtonText;
            set => this.RaiseAndSetIfChanged(ref _createButtonText, value);
        }

        private bool _bottomMessageActive;
        public bool BottomMessageActive
        {
            get => _bottomMessageActive;
            set => this.RaiseAndSetIfChanged(ref _bottomMessageActive, value);
        }
        
        public string SubHeadingText => $"You can add {MAX_PEOPLE_IN_GROUP - _checkedUsers.Count - _existingUsersInGroup - 1} more friends.";
        
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

        private readonly IGroupService _groupService;
        private readonly Action<GroupDMSimple> _onCreateSuccessCallback;
        
        private List<UserSimple> _allFriends;
        private List<UserSimple> _checkedUsers;
        private int _existingUsersInGroup;
        private bool _isCreating;
        private string _groupID;
        
        public CreateGroupDMViewModel(List<UserSimple> allFriends, IGroupService groupService, Action<GroupDMSimple> onCreateSuccessCallback)
        {
            _isCreating = true;
            _existingUsersInGroup = 0;
            CreateButtonText = "Create DM";
            
            _groupService = groupService;
            _onCreateSuccessCallback = onCreateSuccessCallback;
            
            _allFriends = allFriends.OrderBy(friend => friend.UserName).ToList();
            _checkedUsers = new List<UserSimple>();
            
            SelectedUsers = new ObservableCollection<CreateGroupDMSelectedUserViewModel>();
            Users = new ObservableCollection<CreateGroupDMUserViewModel>();

            BottomMessageActive = false;
            UsernameField = string.Empty;
            UpdateUserList();

            CreateDMCommand = ReactiveCommand.Create(OnClick_CreateDM);
        }

        public void SetupAddToGroup(string groupID, List<UserSimple> existingUsers)
        {
            _isCreating = false;
            _existingUsersInGroup = existingUsers.Count;
            _groupID = groupID;
            CreateButtonText = "Add User(s)";
            
            HashSet<string> existingUserIDs = existingUsers.Select(user => user.UserID).ToHashSet();
            _allFriends = _allFriends.Where(user => !existingUserIDs.Contains(user.UserID)).ToList();
            
            UpdateUserList();
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
            if (_checkedUsers.Count == 0)
                return;
            
            // create group dm
            List<string> friends = _checkedUsers.Select(user => user.UserID).ToList();

            bool success = false;
            string message = string.Empty;
            GroupDMSimple groupDM = null;

            if (_isCreating)
            {
                var response = await _groupService.CreateGroupDM(friends);
                success = response.success;
                groupDM = response.groupDMSimple;
                message = response.message;
            }
            else
            {
                var response = await _groupService.AddFriendsToGroupDM(_groupID, friends);
                success = response.success;
                groupDM = response.groupDMSimple;
                message = response.message;
            }
            
            _checkedUsers = new List<UserSimple>();
            SelectedUsers = new ObservableCollection<CreateGroupDMSelectedUserViewModel>();
            Users = new ObservableCollection<CreateGroupDMUserViewModel>();

            if (success == false)
            {
                BottomMessageActive = true;
                BottomMessageField = message;
                return;
            }
            
            _onCreateSuccessCallback?.Invoke(groupDM);
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
