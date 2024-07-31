using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class SelectUsersSelectedUserView : UserControl
    {
        private readonly Color NormalColor = Color.Parse("#313338");
        private readonly Color HoveredColor = Color.Parse("#282a2e");
        
        private bool _hovered = false;
        
        public SelectUsersSelectedUserView()
        {
            InitializeComponent();
        }
        
        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            _hovered = true;
            ((Border)sender).Background = new SolidColorBrush(HoveredColor);
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            _hovered = false;
            ((Border)sender).Background = new SolidColorBrush(NormalColor);
        }
        
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (_hovered && DataContext is SelectUsersSelectedUserViewModel viewModel)
            {
                if (viewModel?.OnClickCommand.CanExecute(null) == true)
                {
                    viewModel.OnClickCommand.Execute(null);
                }
            }
        }
    }   
}