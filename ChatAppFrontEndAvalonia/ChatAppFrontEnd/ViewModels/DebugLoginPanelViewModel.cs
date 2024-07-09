using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ChatApp.Shared.TableDataSimple;
using ReactiveUI;

namespace ChatAppFrontEnd.ViewModels
{
    public class DebugLoginPanelViewModel : ViewModelBase
    {
        private ObservableCollection<CreateGroupDMSelectedUserViewModel> _users;
        public ObservableCollection<CreateGroupDMSelectedUserViewModel> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        private readonly Action<string, string> _loginCallback;
        
        public DebugLoginPanelViewModel(Action<string, string> loginCallback)
        {
            _loginCallback = loginCallback;

            Users = new ObservableCollection<CreateGroupDMSelectedUserViewModel>()
            {
                new (new UserSimple() { UserName = "errol", UserID = "pass" }, OnClick_User),
                new (new UserSimple() { UserName = "errol2", UserID = "pass" }, OnClick_User),
                new (new UserSimple() { UserName = "errol3", UserID = "pass" }, OnClick_User),
                new (new UserSimple() { UserName = "ava", UserID = "pass" }, OnClick_User),
                new (new UserSimple() { UserName = "ava2", UserID = "pass" }, OnClick_User),
                new (new UserSimple() { UserName = "ava3", UserID = "pass" }, OnClick_User),
            };
        }

        private void OnClick_User(CreateGroupDMSelectedUserViewModel viewModel)
        {
            _loginCallback.Invoke(viewModel.User.UserName, viewModel.User.UserID);
        }
    }
}

