using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Source.Services;
using ChatAppFrontEnd.ViewModels.Logic;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class SelectUsersViewModel : ViewModelBase
    {
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
        
        private string _confirmButtonText;
        public string ConfirmButtonText
        {
            get => _confirmButtonText;
            set => this.RaiseAndSetIfChanged(ref _confirmButtonText, value);
        }

        private bool _bottomMessageActive;
        public bool BottomMessageActive
        {
            get => _bottomMessageActive;
            set => this.RaiseAndSetIfChanged(ref _bottomMessageActive, value);
        }

        private string _headingText;
        public string HeadingText
        {
            get => _headingText;
            set => this.RaiseAndSetIfChanged(ref _headingText, value);
        }
        
        private ObservableCollection<SelectUsersSelectedUserViewModel> _selectedUsers;
        public ObservableCollection<SelectUsersSelectedUserViewModel> SelectedUsers
        {
            get => _selectedUsers;
            set => this.RaiseAndSetIfChanged(ref _selectedUsers, value);
        }
        
        private ObservableCollection<SelectUsersUserViewModel> _users;
        public ObservableCollection<SelectUsersUserViewModel> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }
        
        public string SubHeadingText => $"You can add {_logic.MaxAmount - _checkedUsers.Count - _logic.AmountDifference - 1} more {_subHeadingSuffix}.";
        
        public ICommand ConfirmCommand { get; }

        private readonly SelectUsersLogicBase _logic;
        private readonly List<UserSimple> _allUsers;
        private readonly string _subHeadingSuffix;
        
        private List<UserSimple> _checkedUsers;
        
        public SelectUsersViewModel(List<UserSimple> allUsers, string headingText, string confirmButtonText, SelectUsersLogicBase logic, string subHeadingSuffix = "friends")
        {
            _logic = logic;
            _subHeadingSuffix = subHeadingSuffix;
            
            HeadingText = headingText;
            ConfirmButtonText = confirmButtonText;
            
            _allUsers = allUsers.OrderBy(friend => friend.UserName).ToList();
            _checkedUsers = new List<UserSimple>();
            
            SelectedUsers = new ObservableCollection<SelectUsersSelectedUserViewModel>();
            Users = new ObservableCollection<SelectUsersUserViewModel>();

            BottomMessageActive = false;
            UsernameField = string.Empty;
            UpdateUserList();

            ConfirmCommand = ReactiveCommand.Create(OnClick_Confirm);
        }

        public void UpdateUserList()
        {
            Users = new ObservableCollection<SelectUsersUserViewModel>();
            
            foreach (UserSimple user in _allUsers)
            {
                if (!user.UserName.StartsWith(UsernameField, false, null))
                    continue;
                
                Users.Add(new SelectUsersUserViewModel(user, OnUserCheckChanged) { Checked = _checkedUsers.Contains(user) });
            }
        }

        private async void OnClick_Confirm()
        {
            if (_checkedUsers.Count == 0)
                return;
            
            List<string> users = _checkedUsers.Select(user => user.UserID).ToList();

            var response = await _logic.HandleConfirm(users);

            _checkedUsers = new List<UserSimple>();
            SelectedUsers = new ObservableCollection<SelectUsersSelectedUserViewModel>();
            Users = new ObservableCollection<SelectUsersUserViewModel>();

            if (response.result == false)
            {
                BottomMessageActive = true;
                BottomMessageField = response.message;
                return;
            }
            
            _logic.OnSuccess();
        }

        private void OnUserCheckChanged(SelectUsersUserViewModel userViewModel)
        {
            UserSimple user = userViewModel.User;
            
            if (userViewModel.Checked) // removing check
            { 
                _checkedUsers.Remove(user);
                
                SelectUsersSelectedUserViewModel selectedUser = SelectedUsers.FirstOrDefault(u => u.User.UserID == user.UserID);
                if (selectedUser != null)
                    SelectedUsers.Remove(selectedUser);
            }
            else // checking
            { 
                if (_checkedUsers.Count >= _logic.MaxAmount - 1)
                    return;
                
                SelectedUsers.Add(new SelectUsersSelectedUserViewModel(user, OnRemoveSelectedUser));
                _checkedUsers.Add(user);
            }

            _checkedUsers = _checkedUsers.OrderBy(friend => friend.UserName).ToList();

            this.RaisePropertyChanged(nameof(SubHeadingText));
            UpdateUserList();
        }

        private void OnRemoveSelectedUser(SelectUsersSelectedUserViewModel selectedUserViewModel)
        {
            _checkedUsers.Remove(selectedUserViewModel.User);
            SelectedUsers.Remove(selectedUserViewModel);
            
            this.RaisePropertyChanged(nameof(SubHeadingText));
            UpdateUserList();
        }
    }
}
