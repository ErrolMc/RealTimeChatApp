using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ChatAppFrontEnd.ViewModels;

namespace ChatAppFrontEnd.Views
{
    public partial class RightClickMenuButtonView : UserControl
    {
        private readonly Color NormalColor = Color.Parse("#111214");
        private readonly Color HoveredColor = Color.Parse("#505cdc");
        private readonly Color PressedColor = Color.Parse("#3c45a5");
        
        private bool _hovered = false;
        
        public RightClickMenuButtonView()
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

            if (_hovered && this.DataContext is RightClickMenuButtonViewModel viewModel && this.VisualRoot is Visual window)
            {
                if (e.InitialPressMouseButton != MouseButton.Left)
                    return;

                Point mousePos = e.GetPosition(window);
                viewModel?.OnClick(mousePos);
            }
        }
    }
}

