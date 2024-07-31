using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class SelectUsersView : UserControl
    {
        public SelectUsersView()
        {
            InitializeComponent();
        }

        private void OnUsernameTextChanged(object sender, TextChangedEventArgs e)
        {
            var viewModel = DataContext as SelectUsersViewModel;
            viewModel?.UpdateUserList();
        }
    }   
}