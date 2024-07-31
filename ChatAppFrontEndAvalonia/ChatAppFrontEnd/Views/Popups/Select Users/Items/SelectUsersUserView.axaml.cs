using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class SelectUsersUserView : UserControl
    {
        private readonly Color NormalColor = Color.Parse("#313338");
        private readonly Color HoveredColor = Color.Parse("#393c41");
        private readonly Color PressedColor = Color.Parse("#45474b");
        
        private bool _hovered = false;
    
        public SelectUsersUserView()
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
            ((Border)sender).Background = new SolidColorBrush(PressedColor);
        }
        
        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ((Border)sender).Background = new SolidColorBrush(_hovered ? HoveredColor : NormalColor);
            
            // this definitely breaks MVVM, but I couldn't figure out any other way 
            // to change the color on release and call the OnClickCommand
            if (_hovered && DataContext is SelectUsersUserViewModel viewModel)
            {
                if (viewModel?.OnClickCommand.CanExecute(null) == true)
                {
                    viewModel.OnClickCommand.Execute(null);
                }
            }
        }
    }
}

