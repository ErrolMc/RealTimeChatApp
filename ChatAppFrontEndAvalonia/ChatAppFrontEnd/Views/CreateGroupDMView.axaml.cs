using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class CreateGroupDMView : UserControl
    {
        public CreateGroupDMView()
        {
            InitializeComponent();
        }

        private void OnUsernameTextChanged(object sender, TextChangedEventArgs e)
        {
            var viewModel = DataContext as CreateGroupDMViewModel;
            viewModel?.OnUsernameTextChanged();
        }
    }   
}