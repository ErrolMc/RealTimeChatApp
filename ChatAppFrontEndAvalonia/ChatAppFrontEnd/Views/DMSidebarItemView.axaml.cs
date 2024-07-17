using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class DMSidebarItemView : UserControl
    {
        private readonly Color NormalColor = Color.Parse("#2b2d31");
        private readonly Color HoveredColor = Color.Parse("#35373c");
        private readonly Color PressedColor = Color.Parse("#3b3d44");

        private bool _hovered = false;
        
        public DMSidebarItemView()
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
            if (_hovered && this.DataContext is DMSidebarItemViewModel viewModel)
            {
                bool leftClick = e.InitialPressMouseButton == MouseButton.Left;
                if (leftClick)
                    viewModel?.OnLeftClick();
                else
                    viewModel?.OnRightClick();
            }
        }
    }
}
